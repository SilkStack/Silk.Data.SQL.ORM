using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Modelling.Conventions;
using Silk.Data.SQL.ORM.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Silk.Data.SQL.ORM
{
	/// <summary>
	/// Describes the types, models and resource loaders of a data domain.
	/// </summary>
	public class DataDomain
	{
		private static EnumerableConversionsConvention _bindEnumerableConversions = new EnumerableConversionsConvention();

		private static IEnumerable<EntityModel> MakeEntityModels(DomainDefinition domainDefinition, DataDomain dataDomain)
		{
			foreach (var schemaDefinition in domainDefinition.SchemaDefinitions)
			{
				Type entityModelType;
				if (schemaDefinition.ProjectionType == null)
					entityModelType = typeof(EntityModel<>)
						.MakeGenericType(schemaDefinition.EntityType);
				else
					entityModelType = typeof(EntityModel<,>)
						.MakeGenericType(schemaDefinition.EntityType, schemaDefinition.ProjectionType);

				var ctor = entityModelType.GetTypeInfo().DeclaredConstructors.First(q => q.GetParameters().Length == 0);
				var entityModel = ctor.Invoke(null) as EntityModel;
				entityModel.Name = entityModelType.Name;
				entityModel.SetModel(TypeModeller.GetModelOf(schemaDefinition.EntityType));
				entityModel.Domain = dataDomain;
				yield return entityModel;
			}
		}

		private static IEnumerable<MutableDataField> MakeDataFields(DomainDefinition domainDefinition, EntityModel entityModel)
		{
			var schema = domainDefinition.SchemaDefinitions.FirstOrDefault(q => q.EntityType == entityModel.EntityType);
			if (schema != null)
			{
				var entityTable = schema.TableDefinitions.FirstOrDefault(q => q.IsEntityTable);
				if (entityTable != null)
				{
					foreach (var field in entityTable.Fields)
					{
						yield return new MutableDataField(field.Name, field.DataType, field.ModelBinding, field.Metadata.ToArray());
					}
				}
			}
		}

		private static IEnumerable<DataField> EntityTableFields(EntityModel model)
		{
			foreach (var field in model.Fields)
			{
				var relationshipDefinition = field.Metadata.OfType<RelationshipDefinition>()
					.FirstOrDefault();
				if (relationshipDefinition == null)
					yield return field;
			}
		}

		private static EntitySchema MakeDataSchema(EntityModel entityModel, DomainDefinition domainDefinition)
		{
			var schemaDefinition = domainDefinition.SchemaDefinitions.FirstOrDefault(q => q.EntityType == entityModel.EntityType);
			if (schemaDefinition == null)
				return null;

			var schema = new EntitySchema();
			foreach (var tableDefinition in schemaDefinition.TableDefinitions)
			{
				if (tableDefinition.IsEntityTable)
				{
					var table = new Table(tableDefinition.TableName, true,
						EntityTableFields(entityModel).ToArray()
						);
					foreach(var field in table.DataFields.OfType<MutableDataField>())
					{
						field.Table = table;
					}
					schema.AddTable(table);
				}
				else
				{
					var table = new Table();
					var fields = tableDefinition.Fields.Select(fieldDefintion =>
							new DataField(fieldDefintion.Name, fieldDefintion.DataType, fieldDefintion.Metadata.ToArray(),
								fieldDefintion.ModelBinding, table, null)
						).ToArray();
					table.Initialize(tableDefinition.TableName, false, fields);
					schema.AddTable(table);
				}
			}
			return schema;
		}

		private static void ConstructRelationship(EntityModel entityModel, MutableDataField dataField,
			DomainDefinition domainDefinition, DataDomain dataDomain)
		{
			var schemaDefinition = domainDefinition.SchemaDefinitions.FirstOrDefault(q => q.EntityType == entityModel.EntityType);
			if (schemaDefinition == null)
				return;

			var entityTableDefinition = schemaDefinition.TableDefinitions.FirstOrDefault(q => q.IsEntityTable);
			if (entityTableDefinition == null)
				return;

			var fieldDefinition = entityTableDefinition.Fields.FirstOrDefault(q => q.Name == dataField.Name);
			if (fieldDefinition == null)
				return;

			var relationshipDefinition = fieldDefinition.Metadata.OfType<RelationshipDefinition>().FirstOrDefault();
			if (relationshipDefinition == null)
				return;

			var relatedToEntity = dataDomain.DataModels.FirstOrDefault(q => q.EntityType == relationshipDefinition.EntityType);
			if (relatedToEntity == null)
				return;

			var relationship = new DataRelationship(relatedToEntity, relationshipDefinition.RelationshipType);

			if (relationshipDefinition.TableReferences != null)
			{
				foreach (var tableReference in relationshipDefinition.TableReferences)
				{
					if (tableReference.EntityTableReferences == null)
						continue;
					foreach (var entityTableReference in tableReference.EntityTableReferences)
					{
						var relatedDataField = new MutableDataField(entityTableReference.Name, entityTableReference.DataType,
							entityTableReference.ModelBinding, entityTableReference.Metadata.Concat(new object[] { new IsNullableAttribute(true) }).ToArray());
						relatedDataField.Table = entityModel.Schema.EntityTable;
						relatedDataField.SetStorage();
						relatedDataField.SetRelationship(relationship);
						entityModel.Schema.EntityTable.InternalDataFields.Add(relatedDataField);
						entityModel.AddField(relatedDataField);
					}
				}
			}

			dataField.SetRelationship(relationship);
		}

		public static DataDomain CreateFromDefinition(DomainDefinition domainDefinition,
			IEnumerable<ViewConvention> viewConventions)
		{
			var dataDomain = new DataDomain(domainDefinition);

			var entityModels = MakeEntityModels(domainDefinition, dataDomain).ToArray();
			foreach (var entityModel in entityModels)
			{
				dataDomain.AddEntityModel(entityModel);

				var entityDataFields = MakeDataFields(domainDefinition, entityModel).ToArray();
				entityModel.Fields = entityDataFields;
				entityModel.PrimaryKeyFields = entityDataFields.Where(q => q.Metadata.OfType<PrimaryKeyAttribute>().Any()).ToArray();

				entityModel.Schema = MakeDataSchema(entityModel, domainDefinition);
			}

			foreach (var entityModel in entityModels)
			{
				entityModel.SetResourceLoaders();
				foreach (var field in entityModel.Fields.OfType<MutableDataField>())
				{
					ConstructRelationship(entityModel, field, domainDefinition, dataDomain);
					field.SetStorage();
				}
			}

			return dataDomain;
		}

		private readonly List<EntityModel> _entityModels = new List<EntityModel>();
		private readonly DomainDefinition _domainDefinition;
		private readonly Dictionary<Type, TableDefinition> _entitySchemaDefinitions = new Dictionary<Type, TableDefinition>();
		private readonly Dictionary<string, EntityModel> _projectionModelCache = new Dictionary<string, EntityModel>();
		private readonly ViewConvention[] _projectionConventions = new ViewConvention[]
		{
			new CopyPrimitiveTypesConvention(),
			new FlattenSimpleTypesConvention(),
			new CopyReferencesConvention(),
			new ProjectSubObjectProperties()
		};

		public IReadOnlyCollection<EntityModel> DataModels => _entityModels;

		private DataDomain(DomainDefinition domainDefinition)
		{
			_domainDefinition = domainDefinition;
		}

		public DataDomain(IEnumerable<EntityModel> entityModels, DomainDefinition domainDefinition)
		{
			domainDefinition.IsReadOnly = true;

			_entityModels = entityModels.ToList();
			_domainDefinition = domainDefinition;
		}

		private void AddEntityModel(EntityModel entityModel)
		{
			_entityModels.Add(entityModel);
		}

		public EntityModel<TSource> GetEntityModel<TSource>()
			where TSource : new()
		{
			return _entityModels.OfType<EntityModel<TSource>>().FirstOrDefault();
		}

		public EntityModel<TView> GetProjectionModel<TSource, TView>()
			where TSource : new()
			where TView : new()
		{
			return GetProjectionModel(typeof(TSource), typeof(TView)) as EntityModel<TView>;
		}

		private EntityModel GetProjectionModel(Type sourceType, Type viewType)
		{
			var entityModel = _entityModels.FirstOrDefault(q => q.EntityType == sourceType);
			if (entityModel == null)
				throw new InvalidOperationException("Entity type not present in data domain.");

			var cacheKey = $"{sourceType.FullName} to {viewType.FullName}";
			if (_projectionModelCache.TryGetValue(cacheKey, out var ret))
				return ret as EntityModel;

			lock (_projectionModelCache)
			{
				if (_projectionModelCache.TryGetValue(cacheKey, out ret))
					return ret;
			}

			var modelOfViewType = TypeModeller.GetModelOf(viewType);
			var targetModel = entityModel.GetAsModel();

			var viewBuilder = new DataViewBuilder(modelOfViewType, targetModel, _projectionConventions, _domainDefinition,
				sourceType, viewType);
			viewBuilder.ProcessModel(targetModel);

			var ctor = typeof(EntityModel<>).MakeGenericType(viewType)
				.GetTypeInfo().DeclaredConstructors.First(q => q.GetParameters().Length == 0);
			ret = ctor.Invoke(null) as EntityModel;

			var fields = new List<DataField>();
			foreach (var fieldDefinition in viewBuilder.ViewDefinition.FieldDefinitions)
			{
				var entityField = entityModel.Fields.FirstOrDefault(
					q => q.ModelBinding?.ViewFieldPath != null &&
						fieldDefinition.ModelBinding?.ViewFieldPath != null &&
						q.ModelBinding.ViewFieldPath.SequenceEqual(fieldDefinition.ModelBinding.ViewFieldPath)
					);
				if (entityField == null)
					continue;
				var relationship = GetProjectionFieldRelationship(fieldDefinition);
				fields.Add(
					new DataField(entityField.Storage?.ColumnName, fieldDefinition.DataType,
						fieldDefinition.Metadata.Concat(entityField.Metadata).ToArray(),
						fieldDefinition.ModelBinding, entityField.Storage?.Table, relationship, fieldDefinition.Name)
					);
			}

			ret.Initalize(viewBuilder.ViewDefinition.Name, TypeModeller.GetModelOf(viewType),
				entityModel.Schema, fields.ToArray(), this);

			_projectionModelCache.Add(cacheKey, ret);

			return ret;
		}

		private DataRelationship GetProjectionFieldRelationship(ViewFieldDefinition fieldDefinition)
		{
			var relationshipDefinition = fieldDefinition.Metadata.OfType<RelationshipDefinition>().FirstOrDefault();
			if (relationshipDefinition == null)
				return null;
			return new DataRelationship(
				_entityModels.FirstOrDefault(q => q.EntityType == relationshipDefinition.EntityType),
				GetProjectionModel(relationshipDefinition.EntityType, relationshipDefinition.ProjectionType),
				relationshipDefinition.RelationshipType
				);
		}

		public QueryCollection Insert<TSource>(params TSource[] sources)
			where TSource : new()
		{
			return new QueryCollection(this)
				.Insert<TSource>(sources);
		}

		public QueryCollection Insert<TSource, TView>(params TView[] sources)
			where TSource : new()
			where TView : new()
		{
			return new QueryCollection(this)
				.Insert<TSource, TView>(sources);
		}

		public QueryCollection Update<TSource>(params TSource[] sources)
			where TSource : new()
		{
			return new QueryCollection(this)
				.Update<TSource>(sources);
		}

		public QueryCollection Update<TSource, TView>(params TView[] sources)
			where TSource : new()
			where TView : new()
		{
			return new QueryCollection(this)
				.Update<TSource, TView>(sources);
		}

		public QueryCollection Delete<TSource>(params TSource[] sources)
			where TSource : new()
		{
			return new QueryCollection(this)
				.Delete<TSource>(sources);
		}

		public QueryCollection Delete<TSource, TView>(params TView[] sources)
			where TSource : new()
			where TView : new()
		{
			return new QueryCollection(this)
				.Delete<TSource, TView>(sources);
		}

		public QueryCollection Delete<TSource>(QueryExpression where)
			where TSource : new()
		{
			return new QueryCollection(this)
				.Delete<TSource>(where);
		}

		public QueryCollection<TSource> Select<TSource>(QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null)
			where TSource : new()
		{
			return new QueryCollection(this)
				.Select<TSource>(where, having, orderBy, groupBy, offset, limit);
		}

		public QueryCollection<TView> Select<TSource, TView>(QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null)
			where TSource : new()
			where TView : new()
		{
			return new QueryCollection(this)
				.Select<TSource, TView>(where, having, orderBy, groupBy, offset, limit);
		}
	}
}
