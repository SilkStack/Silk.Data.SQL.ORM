using System.Collections.Generic;

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
			return null;
		}
	}
}
