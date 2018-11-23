using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Data schema.
	/// </summary>
	public class Schema
	{
		private readonly Dictionary<Type, EntitySchema> _entitySchemas;
		private readonly Dictionary<MethodInfo, IMethodCallConverter> _methodCallConverters;
		private readonly ConditionalWeakTable<ISchemaField, FieldOperations> _fieldOperations
			= new ConditionalWeakTable<ISchemaField, FieldOperations>();

		public MappingOptions ProjectionMappingOptions { get; }

		public Schema(IEnumerable<EntitySchema> entitySchemas,
			Dictionary<MethodInfo, IMethodCallConverter> methodCallConverters,
			MappingOptions projectionMappingOptions,
			Dictionary<ISchemaField, FieldOperations> fieldOperations)
		{
			if (projectionMappingOptions == null)
				throw new ArgumentNullException(nameof(projectionMappingOptions));
			ProjectionMappingOptions = projectionMappingOptions;

			_entitySchemas = entitySchemas.ToDictionary(q => q.EntityType);
			foreach(var schema in _entitySchemas.Values)
			{
				schema.Schema = this;
			}
			_methodCallConverters = methodCallConverters;

			foreach (var kvp in fieldOperations)
			{
				_fieldOperations.Add(kvp.Key, kvp.Value);
			}
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
			where T : class
		{
			return GetEntitySchema(typeof(T)) as EntitySchema<T>;
		}

		public FieldOperations GetFieldOperations(ISchemaField schemaField)
		{
			_fieldOperations.TryGetValue(schemaField, out var operations);
			return operations;
		}

		public FieldOperations<T> GetFieldOperations<T>(ISchemaField<T> schemaField)
			where T : class
		{
			_fieldOperations.TryGetValue(schemaField, out var operations);
			return operations as FieldOperations<T>;
		}
	}
}
