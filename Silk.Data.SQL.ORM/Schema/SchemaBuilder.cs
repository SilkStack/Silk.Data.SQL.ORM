using Silk.Data.SQL.ORM.Expressions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Used to configure and build a schema to create queries against.
	/// </summary>
	public class SchemaBuilder
	{
		private readonly Dictionary<Type, EntitySchemaBuilder> _entitySchemaBuilders
			= new Dictionary<Type, EntitySchemaBuilder>();
		private readonly Dictionary<MethodInfo, IMethodCallConverter> _methodCallConverters
			= new Dictionary<MethodInfo, IMethodCallConverter>();

		public SchemaBuilder()
		{
			_methodCallConverters.Add(
				typeof(Enum).GetMethod(nameof(Enum.HasFlag)), new HasFlagCallConverter()
				);
			_methodCallConverters.Add(
				typeof(DatabaseFunctions).GetMethod(nameof(DatabaseFunctions.Like)), new StringLikeCallConverter()
				);
			_methodCallConverters.Add(
				typeof(DatabaseFunctions).GetMethod(nameof(DatabaseFunctions.Alias)), new AliasCallConverter()
				);
			_methodCallConverters.Add(
				typeof(DatabaseFunctions).GetMethod(nameof(DatabaseFunctions.Count)), new CountCallConverter()
				);
			_methodCallConverters.Add(
				typeof(DatabaseFunctions).GetMethod(nameof(DatabaseFunctions.Random)), new RandomCallConverter()
				);
			_methodCallConverters.Add(
				typeof(DatabaseFunctions).GetMethod(nameof(DatabaseFunctions.IsIn)), new IsInCallConverter()
				);
		}

		public void AddMethodConverter(MethodInfo methodInfo, IMethodCallConverter methodCallConverter)
		{
			_methodCallConverters.Add(methodInfo, methodCallConverter);
		}

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

		public EntitySchemaBuilder<T> DefineEntity<T>(Action<EntitySchemaBuilder<T>> configureCallback)
			where T : class
		{
			var builder = DefineEntity<T>();
			configureCallback?.Invoke(builder);
			return builder;
		}

		/// <summary>
		/// Build the schema.
		/// </summary>
		/// <returns></returns>
		public virtual Schema Build()
		{
			var entityPrimitiveFields = BuildEntityPrimitiveFields();
			while (DefineNewFields(entityPrimitiveFields)) { }
			var entitySchemas = BuildEntitySchemas(entityPrimitiveFields);
			return new Schema(entitySchemas, _methodCallConverters);
		}

		private bool DefineNewFields(PartialEntitySchemaCollection partialEntitySchemas)
		{
			var result = false;
			foreach (var kvp in _entitySchemaBuilders)
			{
				if (kvp.Value.DefineNewSchemaFields(partialEntitySchemas))
					result = true;
			}
			return result;
		}

		private PartialEntitySchemaCollection BuildEntityPrimitiveFields()
		{
			var primitiveFields = new PartialEntitySchemaCollection(_entitySchemaBuilders.Keys);
			foreach (var kvp in _entitySchemaBuilders)
			{
				primitiveFields[kvp.Key] = kvp.Value.BuildPartialSchema(primitiveFields);
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
