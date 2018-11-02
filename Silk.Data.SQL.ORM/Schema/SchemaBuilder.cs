﻿using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Used to configure and build a schema to create queries against.
	/// </summary>
	public class SchemaBuilder
	{
		private readonly Dictionary<Type, (IEntitySchemaDefinition Definition, IEntitySchemaBuilder Builder)> _entitySchemaBuilders
			= new Dictionary<Type, (IEntitySchemaDefinition Definition, IEntitySchemaBuilder Builder)>();
		private readonly Dictionary<MethodInfo, IMethodCallConverter> _methodCallConverters
			= new Dictionary<MethodInfo, IMethodCallConverter>();
		private readonly List<RelationshipBuilder> _relationshipBuilders
			= new List<RelationshipBuilder>();
		private readonly List<IEntitySchemaAssemblage> _entitySchemaAssemblages
			= new List<IEntitySchemaAssemblage>();

		public MappingOptions ProjectionMappingOptions { get; set; }
			= MappingOptions.CreateObjectMappingOptions();

		public SchemaBuilder()
		{
			AddMethodConverter(
				typeof(Enum).GetMethod(nameof(Enum.HasFlag)), new HasFlagCallConverter()
				);
			AddMethodConverter(
				typeof(DatabaseFunctions).GetMethod(nameof(DatabaseFunctions.Like)), new StringLikeCallConverter()
				);
			AddMethodConverter(
				typeof(DatabaseFunctions).GetMethod(nameof(DatabaseFunctions.Alias)), new AliasCallConverter()
				);
			AddMethodConverter(
				typeof(DatabaseFunctions).GetMethod(nameof(DatabaseFunctions.Count)), new CountCallConverter()
				);
			AddMethodConverter(
				typeof(DatabaseFunctions).GetMethod(nameof(DatabaseFunctions.Random)), new RandomCallConverter()
				);
			AddMethodConverter(
				typeof(DatabaseFunctions).GetMethod(nameof(DatabaseFunctions.IsIn)), new IsInCallConverter()
				);
		}

		public void AddMethodConverter(MethodInfo methodInfo, IMethodCallConverter methodCallConverter)
		{
			_methodCallConverters.Add(methodInfo, methodCallConverter);
		}

		public RelationshipBuilder<TLeft, TRight> DefineRelationship<TLeft, TRight>(string name)
			where TLeft : class
			where TRight : class
		{
			var relationship = _relationshipBuilders.FirstOrDefault(q =>
				q.Left == typeof(TLeft) && q.Right == typeof(TRight) && q.Name == name);
			if (relationship == null)
			{
				relationship = new RelationshipBuilder<TLeft, TRight>
				{
					Name = name
				};
				_relationshipBuilders.Add(relationship);
			}
			return relationship as RelationshipBuilder<TLeft, TRight>;
		}

		public RelationshipBuilder<TLeft, TRight> DefineRelationship<TLeft, TRight>(string name, Action<RelationshipBuilder<TLeft, TRight>> callback)
			where TLeft : class
			where TRight : class
		{
			var relationship = DefineRelationship<TLeft, TRight>(name);
			callback?.Invoke(relationship);
			return relationship;
		}

		/// <summary>
		/// Add an entity type to the schema and return the EntitySchemaBuilder for customizing how the entity is stored.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public EntitySchemaDefinition<T> DefineEntity<T>()
			where T : class
		{
			var entityType = typeof(T);
			if (_entitySchemaBuilders.TryGetValue(entityType, out var definitionBuilderPair))
				return definitionBuilderPair.Definition as EntitySchemaDefinition<T>;
			var definition = new EntitySchemaDefinition<T>();
			var builder = new EntitySchemaBuilder<T>(definition, _entitySchemaAssemblages);
			definitionBuilderPair = (definition, builder);
			_entitySchemaBuilders.Add(entityType, definitionBuilderPair);
			return definition;
		}

		public EntitySchemaDefinition<T> DefineEntity<T>(Action<EntitySchemaDefinition<T>> configureCallback)
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
			_entitySchemaAssemblages.AddRange(CreateEntitySchemaAssemblages());
			foreach (var assemblage in _entitySchemaAssemblages)
			{

			}
			var entitySchemas = _entitySchemaAssemblages.Select(q => q.Builder.BuildSchema())
				.ToArray();
			return new Schema(
				entitySchemas, _methodCallConverters,
				new Relationship[0], ProjectionMappingOptions
				);
		}

		private IEnumerable<IEntitySchemaAssemblage> CreateEntitySchemaAssemblages()
		{
			foreach (var kvp in _entitySchemaBuilders)
			{
				yield return kvp.Value.Builder.CreateAssemblage();
			}
		}
	}
}
