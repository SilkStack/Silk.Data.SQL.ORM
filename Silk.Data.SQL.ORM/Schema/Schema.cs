using Silk.Data.SQL.ORM.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Data schema.
	/// </summary>
	public class Schema
	{
		private readonly Dictionary<Type, EntitySchema> _entitySchemas;
		private readonly Dictionary<MethodInfo, IMethodCallConverter> _methodCallConverters;

		public Schema(IEnumerable<EntitySchema> entitySchemas,
			Dictionary<MethodInfo, IMethodCallConverter> methodCallConverters)
		{
			_entitySchemas = entitySchemas.ToDictionary(q => q.EntityType);
			foreach(var schema in _entitySchemas.Values)
			{
				schema.Schema = this;
			}
			_methodCallConverters = methodCallConverters;
		}

		public IMethodCallConverter GetMethodCallConverter(MethodInfo methodInfo)
		{
			if (methodInfo.IsGenericMethod)
				methodInfo = methodInfo.GetGenericMethodDefinition();
			_methodCallConverters.TryGetValue(methodInfo, out var converter);
			return converter;
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
