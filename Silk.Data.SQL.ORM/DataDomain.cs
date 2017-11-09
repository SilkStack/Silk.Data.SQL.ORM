using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM
{
	/// <summary>
	/// Describes the types, models and resource loaders of a data domain.
	/// </summary>
	public class DataDomain
	{
		private static EnumerableConversionsConvention _bindEnumerableConversions = new EnumerableConversionsConvention();

		public static DataDomain CreateFromDefinition(DomainDefinition domainDefinition,
			IEnumerable<ViewConvention> viewConventions)
		{
			DataDomain builtDomain = null;
			var lazyDomainAccessor = new Lazy<DataDomain>(() => builtDomain);

			var schemas = new List<EntitySchema>();
			var entityModels = new List<EntityModel>();
			foreach (var schemaDefinition in domainDefinition.SchemaDefinitions)
			{
				Type entityModelType;
				if (schemaDefinition.ProjectionType == null)
					entityModelType = typeof(EntityModel<>)
						.MakeGenericType(schemaDefinition.EntityType);
				else
					entityModelType = typeof(EntityModel<,>)
						.MakeGenericType(schemaDefinition.EntityType, schemaDefinition.ProjectionType);

				var entityModel = Activator.CreateInstance(entityModelType, true) as EntityModel;
				entityModel.SetModel(TypeModeller.GetModelOf(schemaDefinition.EntityType));

				var schema = new EntitySchema();

				foreach (var tableDefinition in schemaDefinition.TableDefinitions)
				{
					var table = new Table();
					var fields = new List<DataField>();

					foreach (var fieldDefinition in tableDefinition.Fields)
					{
						var field = new DataField(fieldDefinition.Name, fieldDefinition.DataType,
							fieldDefinition.Metadata.ToArray(), fieldDefinition.ModelBinding, table,
							null, fieldDefinition.ModelFieldName);

						fields.Add(field);
					}
					table.Initialize(tableDefinition.TableName, tableDefinition.IsEntityTable, fields.ToArray());

					schema.AddTable(table);
				}
				entityModel.Initalize(
					schemaDefinition.EntityType.Name, TypeModeller.GetModelOf(schemaDefinition.EntityType),
					schema, schema.Tables.SelectMany(q => q.DataFields),
					lazyDomainAccessor);

				schema.SetEntityModel(entityModel);

				schemas.Add(schema);
				entityModels.Add(entityModel);
			}

			foreach (var schemaDefinition in domainDefinition.SchemaDefinitions)
			{
				foreach (var tableDefinition in schemaDefinition.TableDefinitions)
				{
					foreach (var viewDefinition in tableDefinition.Fields)
					{
						var relationshipDefinition = viewDefinition.Metadata
							.OfType<RelationshipDefinition>()
							.FirstOrDefault();
						if (relationshipDefinition == null)
							continue;

						//  note: the ProjectionType property is invalid here, everything in domain building is bound to full entity schemas
						//        ProjectionType is only valid when making projected views.
						if (relationshipDefinition.ProjectionType != null &&
							relationshipDefinition.ProjectionType != relationshipDefinition.EntityType)
							throw new InvalidOperationException("ProjectionType is only valid on ProjectionModels.");

						var viewField = entityModels
							.Where(q => q.EntityType == schemaDefinition.EntityType)
							.SelectMany(q => q.Fields)
							.Where(q => q.DataType == viewDefinition.DataType && q.Storage.ColumnName == viewDefinition.Name)
							.FirstOrDefault();
						if (viewField == null)
							continue;

						var joinSchema = schemas.FirstOrDefault(q => q.EntityModel.EntityType == relationshipDefinition.EntityType);
						if (joinSchema == null)
							continue;
						//  todo: support schemas were the foreign key may not be in the main entity table?
						var joinedDataField = joinSchema.EntityTable.DataFields
							.FirstOrDefault(q => q.Storage.ColumnName == relationshipDefinition.RelationshipField);
						if (joinedDataField == null)
							continue;

						var relationship = new DataRelationship(joinedDataField, joinSchema.EntityModel,
							relationshipDefinition.RelationshipType);
						viewField.SetRelationship(relationship);
					}
				}
			}

			builtDomain = new DataDomain(schemas, entityModels, viewConventions, domainDefinition);
			return builtDomain;
		}

		private readonly EntitySchema[] _entitySchemas;
		private readonly EntityModel[] _entityModels;
		private readonly ViewConvention[] _viewConventions;
		private readonly DomainDefinition _domainDefinition;
		private readonly Dictionary<Type, TableDefinition> _entitySchemaDefinitions = new Dictionary<Type, TableDefinition>();
		private readonly Dictionary<string, EntityModel> _projectionModelCache = new Dictionary<string, EntityModel>();

		public IReadOnlyCollection<EntityModel> DataModels => _entityModels;

		public DataDomain(IEnumerable<EntitySchema> entitySchemas, IEnumerable<EntityModel> entityModels,
			IEnumerable<ViewConvention> viewConventions, DomainDefinition domainDefinition)
		{
			domainDefinition.IsReadOnly = true;

			_entitySchemas = entitySchemas.ToArray();
			_entityModels = entityModels.ToArray();
			_viewConventions = viewConventions.ToArray();
			_domainDefinition = domainDefinition;
		}

		public EntityModel<TSource> GetEntityModel<TSource>()
			where TSource : new()
		{
			return _entityModels.OfType<EntityModel<TSource>>().FirstOrDefault();
		}

		public EntityModel<TSource,TView> GetProjectionModel<TSource, TView>()
			where TSource : new()
			where TView : new()
		{
			var entityModel = GetEntityModel<TSource>();
			if (entityModel == null)
				throw new InvalidOperationException("Entity type not present in data domain.");

			var cacheKey = $"{typeof(TSource).FullName} to {typeof(TView).FullName}";
			if (_projectionModelCache.TryGetValue(cacheKey, out var ret))
				return ret as EntityModel<TSource,TView>;

			lock (_projectionModelCache)
			{
				if (_projectionModelCache.TryGetValue(cacheKey, out ret))
					return ret as EntityModel<TSource, TView>;

				var lazyDomainAccessor = new Lazy<DataDomain>(() => this);
				var modelOfViewType = TypeModeller.GetModelOf<TView>();

				var viewDefinition = new ViewDefinition(entityModel.Model, modelOfViewType, _viewConventions);
				viewDefinition.UserData.Add(_domainDefinition);
				viewDefinition.UserData.Add(typeof(TSource));
				viewDefinition.UserData.Add(typeof(TView));
				viewDefinition.UserData.Add(lazyDomainAccessor);

				foreach (var field in viewDefinition.TargetModel.Fields)
				{
					foreach (var viewConvention in _viewConventions)
					{
						viewConvention.MakeModelFields(viewDefinition.SourceModel,
							field, viewDefinition);
					}
				}

				_bindEnumerableConversions.FinalizeModel(viewDefinition);

				foreach (var viewConvention in _viewConventions)
				{
					viewConvention.FinalizeModel(viewDefinition);
				}

				var fields = new List<DataField>();
				foreach (var fieldDefinition in viewDefinition.FieldDefinitions)
				{
					var source = entityModel.Fields.FirstOrDefault(q =>
						q.DataType == fieldDefinition.DataType &&
						q.ModelBinding.ModelFieldPath.SequenceEqual(fieldDefinition.ModelBinding.ModelFieldPath)
						);
					if (source != null)
						fields.Add(source);
				}

				ret = new EntityModel<TSource, TView>(viewDefinition.Name, entityModel.Model,
					entityModel.Schema, fields.ToArray(), lazyDomainAccessor);

				_projectionModelCache.Add(cacheKey, ret);

				return ret as EntityModel<TSource, TView>;
			}
		}

		public EntitySchema GetSchema<TSource>()
			where TSource : new()
		{
			return GetSchema(typeof(TSource));
		}

		public EntitySchema GetSchema(Type entityType)
		{
			return _entitySchemas.FirstOrDefault(q => q.EntityModel.EntityType == entityType);
		}
	}
}
