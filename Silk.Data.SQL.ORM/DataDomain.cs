using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Modelling.Conventions;
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
		private static ViewConvention[] _defaultViewConventions = new ViewConvention[]
		{
			new CleanModelNameConvention(),
			new CopySupportedSQLTypesConvention(),
			new IdIsPrimaryKeyConvention(),
			new CopyPrimaryKeyOfTypesWithSchemaConvention(),
			new ProjectReferenceKeysConvention()
		};

		public static DataDomain CreateFromDefinition(DomainDefinition domainDefinition)
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

					foreach (var viewDefinition in tableDefinition.Fields)
					{
						var field = new DataField(viewDefinition.Name, viewDefinition.DataType,
							viewDefinition.Metadata.ToArray(), viewDefinition.ModelBinding, table,
							null, viewDefinition.ModelFieldName);

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

						var relationship = new DataRelationship(joinedDataField, joinSchema.EntityModel);
						viewField.SetRelationship(relationship);
					}
				}
			}

			builtDomain = new DataDomain(schemas, entityModels);
			return builtDomain;
		}

		private readonly EntitySchema[] _entitySchemas;
		private readonly EntityModel[] _entityModels;
		private readonly List<Table> _tables = new List<Table>();

		private readonly Dictionary<Type, TableDefinition> _entitySchemaDefinitions = new Dictionary<Type, TableDefinition>();

		public ViewConvention[] ViewConventions { get; }
		public IReadOnlyCollection<EntityModel> DataModels => _entityModels;
		public IReadOnlyCollection<Table> Tables => _tables;

		public DataDomain(IEnumerable<EntitySchema> entitySchemas, IEnumerable<EntityModel> entityModels)
		{
			_entitySchemas = entitySchemas.ToArray();
			_entityModels = entityModels.ToArray();
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
			return null;
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

		//  OLD API

		/// <summary>
		/// Creates a data model using knowledge from the data domain.
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <returns></returns>
		//public EntityModel<TSource> CreateDataModel<TSource>()
		//	where TSource : new()
		//{
		//	var model = TypeModeller.GetModelOf<TSource>();
		//	var dataModel = model.CreateView(viewDefinition =>
		//	{
		//		return new EntityModel<TSource>(
		//			viewDefinition.Name, model, null,
		//			DataField.FromDefinitions(viewDefinition.UserData.OfType<TableDefinition>(), viewDefinition.FieldDefinitions).ToArray(),
		//			viewDefinition.ResourceLoaders.ToArray(), this);
		//	}, new object[] { this }, ViewConventions);
		//	return dataModel;
		//}

		/// <summary>
		/// Creates a data model using knowledge from the data domain.
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <returns></returns>
		//public EntityModel<TSource, TView> CreateDataModel<TSource, TView>()
		//	where TSource : new()
		//	where TView : new()
		//{
		//	var model = TypeModeller.GetModelOf<TSource>();
		//	var dataModel = model.CreateView(viewDefinition =>
		//	{
		//		return new EntityModel<TSource, TView>(
		//			viewDefinition.Name, model, null,
		//			DataField.FromDefinitions(viewDefinition.UserData.OfType<TableDefinition>(), viewDefinition.FieldDefinitions).ToArray(),
		//			viewDefinition.ResourceLoaders.ToArray(), this);
		//	}, typeof(TView), new object[] { this }, ViewConventions);
		//	return dataModel;
		//}
	}
}
