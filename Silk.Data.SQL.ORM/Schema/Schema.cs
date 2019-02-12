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
		private readonly List<EntitySchema> _entitySchemas = new List<EntitySchema>();
		private readonly Dictionary<MethodInfo, IMethodCallConverter> _methodCallConverters;
		private readonly ConditionalWeakTable<SchemaField, FieldOperations> _fieldOperations
			= new ConditionalWeakTable<SchemaField, FieldOperations>();
		private readonly Dictionary<Guid, EntitySchema> _idIndexedSchemas;

		public MappingOptions ProjectionMappingOptions { get; }

		public Schema(Dictionary<Guid, EntitySchema> entitySchemas,
			Dictionary<MethodInfo, IMethodCallConverter> methodCallConverters,
			MappingOptions projectionMappingOptions,
			Dictionary<SchemaField, FieldOperations> fieldOperations)
		{
			if (projectionMappingOptions == null)
				throw new ArgumentNullException(nameof(projectionMappingOptions));
			ProjectionMappingOptions = projectionMappingOptions;

			_entitySchemas.AddRange(entitySchemas.Values);
			_idIndexedSchemas = entitySchemas;
			foreach(var schema in _entitySchemas)
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

		public EntitySchema<T> GetEntitySchema<T>(EntitySchemaDefinition<T> definition)
			where T : class
		{
			if (_idIndexedSchemas.TryGetValue(definition.DefinitionId, out var entitySchema))
				return entitySchema as EntitySchema<T>;
			return null;
		}

		public EntitySchema GetEntitySchema(Type entityType)
		{
			return _entitySchemas.FirstOrDefault(q => q.EntityType == entityType);
		}

		public EntitySchema<T> GetEntitySchema<T>()
			where T : class
		{
			return GetEntitySchema(typeof(T)) as EntitySchema<T>;
		}

		public FieldOperations GetFieldOperations(SchemaField schemaField)
		{
			_fieldOperations.TryGetValue(schemaField, out var operations);
			return operations;
		}

		public FieldOperations<T> GetFieldOperations<T>(SchemaField<T> schemaField)
			where T : class
		{
			_fieldOperations.TryGetValue(schemaField, out var operations);
			return operations as FieldOperations<T>;
		}
	}
}
