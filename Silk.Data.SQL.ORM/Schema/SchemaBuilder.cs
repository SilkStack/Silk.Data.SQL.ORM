using System;
using System.Collections.Generic;
using System.Linq;
using Silk.Data.SQL.ORM.Modelling;

namespace Silk.Data.SQL.ORM.Schema
{
	public class SchemaBuilder
	{
		private readonly List<EntitySchemaOptions> _entityTypes
			= new List<EntitySchemaOptions>();

		public EntitySchemaOptions<T> DefineEntity<T>()
		{
			var options = new EntitySchemaOptions<T>(this);
			_entityTypes.Add(options);
			return options;
		}

		public EntityModelTransformer GetModelTransformer(Type type)
		{
			var options = _entityTypes.FirstOrDefault(q => q.GetEntityModel().EntityType == type);
			if (options == null)
				return null;
			return options.ModelTransformer;
		}

		public Schema Build()
		{
			var fieldAdded = true;
			while (fieldAdded)
			{
				fieldAdded = false;
				foreach (var entityType in _entityTypes)
				{
					if (entityType.PerformTransformationPass())
						fieldAdded = true;
				}
			}

			var entityModelCollection = new EntityModelCollection(
				_entityTypes.Select(q => q.GetEntityModel())
			);

			var tables = new List<Table>(
				entityModelCollection.Select(q => q.EntityTable)
				);
			var schema = new Schema(entityModelCollection);
			foreach (var fieldWithBuildFinalizer in entityModelCollection.SelectMany(q => q.Fields)
				.OfType<IModelBuildFinalizerField>())
			{
				fieldWithBuildFinalizer.FinalizeModelBuild(schema, tables);
			}

			foreach (var modelWithBuildFinalizer in entityModelCollection.OfType<IModelBuilderFinalizer>())
			{
				modelWithBuildFinalizer.FinalizeBuiltModel(schema, tables);
			}

			schema.SetTables(tables);
			return schema;
		}
	}
}
