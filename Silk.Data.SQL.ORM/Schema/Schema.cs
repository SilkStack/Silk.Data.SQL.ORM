using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Data schema.
	/// </summary>
	public class Schema
	{
		private readonly Dictionary<Type, EntitySchema> _entitySchemas;

		public Schema(IEnumerable<EntitySchema> entitySchemas)
		{
			_entitySchemas = entitySchemas.ToDictionary(q => q.EntityType);
		}

		public EntitySchema GetEntitySchema(Type entityType)
		{
			_entitySchemas.TryGetValue(entityType, out var schema);
			return schema;
		}

		public EntitySchema<T> GetEntitySchema<T>()
		{
			return GetEntitySchema(typeof(T)) as EntitySchema<T>;
		}
	}
}
