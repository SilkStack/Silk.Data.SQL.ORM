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
			var options = new EntitySchemaOptions<T>();
			_entityTypes.Add(options);
			return options;
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

			return new Schema(entityModelCollection);
		}
	}
}
