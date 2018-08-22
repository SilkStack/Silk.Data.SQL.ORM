using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Used to configure and build a schema to create queries against.
	/// </summary>
	public class SchemaBuilder
	{
		private readonly Dictionary<Type, EntitySchemaBuilder> _entitySchemaBuilders
			= new Dictionary<Type, EntitySchemaBuilder>();

		/// <summary>
		/// Add an entity type to the schema and return the EntitySchemaBuilder for customizing how the entity is stored.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public EntitySchemaBuilder<T> DefineEntity<T>()
			where T : class
		{
			var entityType = typeof(T);
			if (_entitySchemaBuilders.TryGetValue(entityType, out var builder))
				return builder as EntitySchemaBuilder<T>;
			builder = new EntitySchemaBuilder<T>();
			_entitySchemaBuilders.Add(entityType, builder);
			return builder as EntitySchemaBuilder<T>;
		}

		/// <summary>
		/// Build the schema.
		/// </summary>
		/// <returns></returns>
		public virtual Schema Build()
		{
			var entityPrimitiveFields = BuildEntityPrimitiveFields();
			var entitySchemas = BuildEntitySchemas(entityPrimitiveFields);
			return new Schema(entitySchemas);
		}

		private PartialEntitySchemaCollection BuildEntityPrimitiveFields()
		{
			var primitiveFields = new PartialEntitySchemaCollection();
			foreach (var kvp in _entitySchemaBuilders)
			{
				primitiveFields.Add(kvp.Key, kvp.Value.BuildPartialSchema());
			}
			return primitiveFields;
		}

		private IEnumerable<EntitySchema> BuildEntitySchemas(PartialEntitySchemaCollection entityPrimitiveFields)
		{
			foreach (var kvp in _entitySchemaBuilders)
			{
				yield return kvp.Value.BuildSchema(entityPrimitiveFields);
			}
		}
	}
}
