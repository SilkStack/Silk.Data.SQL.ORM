using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
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
		private readonly List<(IEntitySchemaDefinition Definition, IEntitySchemaBuilder Builder)> _entitySchemaBuilders
			= new List<(IEntitySchemaDefinition Definition, IEntitySchemaBuilder Builder)>();
		private readonly Dictionary<MethodInfo, IMethodCallConverter> _methodCallConverters
			= new Dictionary<MethodInfo, IMethodCallConverter>();
		private readonly List<IEntitySchemaAssemblage> _entitySchemaAssemblages
			= new List<IEntitySchemaAssemblage>();

		public MappingOptions ProjectionMappingOptions { get; set; }
		 = CreateDefaultMappingOptions();

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
			AddMinMethods();
			AddMaxMethods();
		}

		private void AddMinMethods()
		{
			foreach (var methodInfo in typeof(DatabaseFunctions).GetMethods()
				.Where(q => q.Name == nameof(DatabaseFunctions.Min)))
			{
				AddMethodConverter(methodInfo, new MinCallConverter());
			}
		}

		private void AddMaxMethods()
		{
			foreach (var methodInfo in typeof(DatabaseFunctions).GetMethods()
				.Where(q => q.Name == nameof(DatabaseFunctions.Max)))
			{
				AddMethodConverter(methodInfo, new MaxCallConverter());
			}
		}

		public void AddMethodConverter(MethodInfo methodInfo, IMethodCallConverter methodCallConverter)
		{
			_methodCallConverters.Add(methodInfo, methodCallConverter);
		}

		public void AddDefinition<T>(EntitySchemaDefinition<T> definition)
			where T : class
		{
			var builder = new EntitySchemaBuilder<T>(definition, _entitySchemaAssemblages);
			var definitionBuilderPair = (definition, builder);
			_entitySchemaBuilders.Add(definitionBuilderPair);
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
			var definitionBuilderPair = _entitySchemaBuilders.FirstOrDefault(q => q.Definition.EntityType == entityType);
			if (_entitySchemaBuilders.Any(q => q.Definition.EntityType == entityType))
				return definitionBuilderPair.Definition as EntitySchemaDefinition<T>;

			var definition = new EntitySchemaDefinition<T>();
			var builder = new EntitySchemaBuilder<T>(definition, _entitySchemaAssemblages);
			definitionBuilderPair = (definition, builder);
			_entitySchemaBuilders.Add(definitionBuilderPair);
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
				assemblage.Builder.DefineAllStoredFields();
			}
			var entitySchemas = _entitySchemaAssemblages.Select(q => new { Id = q.Definition.DefinitionId, Schema = q.Builder.BuildSchema() })
				.ToDictionary(q => q.Id, q => q.Schema);
			var fieldOperations = _entitySchemaAssemblages.SelectMany(q => q.Builder.BuildFieldOperations())
				.ToDictionary(q => q.Key, q => q.Value);
			return new Schema(
				entitySchemas, _methodCallConverters,
				ProjectionMappingOptions,
				fieldOperations
				);
		}

		private IEnumerable<IEntitySchemaAssemblage> CreateEntitySchemaAssemblages()
		{
			foreach (var pair in _entitySchemaBuilders)
			{
				yield return pair.Builder.CreateAssemblage();
			}
		}

		public static MappingOptions CreateDefaultMappingOptions()
		{
			var ret = new MappingOptions
			{
				BindingCandidatesDelegate = ProjectionBuilder.GetBindCandidatePairs
			};

			ret.Conventions.Add(new UseObjectMappingOverrides());
			ret.Conventions.Add(CreateInstanceAsNeeded.Instance);
			ret.Conventions.Add(MapOverriddenTypes.Instance);

			//  object type conversions
			ret.Conventions.Add(CopyExplicitCast.Instance);
			ret.Conventions.Add(CreateInstancesOfPropertiesAsNeeded.Instance);

			//  straight up copies
			ret.Conventions.Add(CopySameTypes.Instance);

			//  framework types casting
			ret.Conventions.Add(CastNumericTypes.Instance);

			//  string conversions
			ret.Conventions.Add(ConvertToStringWithToString.Instance);
			ret.Conventions.Add(CopyTryParse.Instance);

			return ret;
		}
	}
}
