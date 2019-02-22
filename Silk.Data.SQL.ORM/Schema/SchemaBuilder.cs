using Silk.Data.Modelling;
using Silk.Data.Modelling.GenericDispatch;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Used to configure and build a schema.
	/// </summary>
	public class SchemaBuilder
	{
		private readonly List<EntityDefinition> _entityDefinitions
			= new List<EntityDefinition>();

		private readonly Dictionary<MethodInfo, IMethodCallConverter> _methodCallConverters
			= new Dictionary<MethodInfo, IMethodCallConverter>();

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

		/// <summary>
		/// Define an entity type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="configure"></param>
		/// <returns></returns>
		public SchemaBuilder Define<T>(Action<EntityDefinition<T>> configure = null)
			where T : class
		{
			var definition = new EntityDefinition<T>();
			_entityDefinitions.Add(definition);
			configure?.Invoke(definition);
			return this;
		}

		/// <summary>
		/// Get all the entity definitions for the provided entity type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IEnumerable<EntityDefinition<T>> GetAll<T>()
			where T : class
			=> _entityDefinitions.OfType<EntityDefinition<T>>();

		/// <summary>
		/// Build the schema.
		/// </summary>
		/// <returns></returns>
		public virtual Schema Build()
		{
			return new Schema(
				BuildEntityModels(),
				_methodCallConverters
				);
		}

		protected virtual IEnumerable<EntityModel> BuildEntityModels()
		{
			foreach (var entityDefinition in _entityDefinitions)
			{
				yield return BuildEntityModel(entityDefinition);
			}
		}

		protected virtual EntityModel BuildEntityModel(EntityDefinition entityDefinition)
		{
			EntityFieldBuilder.Reset();
			return entityDefinition.BuildModel(
				BuildEntityFields(entityDefinition, entityDefinition.TypeModel)
				);
		}

		protected virtual IEnumerable<EntityField> BuildEntityFields(
			EntityDefinition entityDefinition,
			TypeModel typeModel,
			IEnumerable<IField> relativeParentFields = null,
			IEnumerable<IField> fullParentFields = null,
			IQueryReference source = null
			)
		{
			if (source == null)
				source = new TableReference(entityDefinition.TableName);

			var builder = new EntityFieldBuilder(entityDefinition, this,
				relativeParentFields ?? new IField[0],
				fullParentFields ?? new IField[0],
				source);
			foreach (var field in typeModel.Fields)
			{
				if (field.IsEnumerableType)
					continue;

				field.Dispatch(builder);
				yield return builder.EntityField;
			}
		}

		private class EntityFieldBuilder : IFieldGenericExecutor
		{
			private static int _joinCount = 1;

			public static void Reset()
			{
				_joinCount = 1;
			}

			private readonly EntityDefinition _entityDefinition;
			private readonly SchemaBuilder _schemaBuilder;
			private readonly IEnumerable<IField> _relativeParentFields;
			private readonly IEnumerable<IField> _fullParentFields;
			private readonly IQueryReference _source;

			public EntityField EntityField { get; private set; }

			public EntityFieldBuilder(EntityDefinition entityDefinition, SchemaBuilder schemaBuilder,
				IEnumerable<IField> relativeParentFields, IEnumerable<IField> fullParentFields,
				IQueryReference source)
			{
				_entityDefinition = entityDefinition;
				_schemaBuilder = schemaBuilder;
				_relativeParentFields = relativeParentFields;
				_fullParentFields = fullParentFields;
				_source = source;
			}

			private EntityField BuildValueField<TData>(IField field)
				=> ValueEntityField<TData>.Create(field, _relativeParentFields, _fullParentFields, _source);

			private EntityField BuildEmbeddedField<TData>(IField field)
				=> EmbeddedEntityField<TData>.Create(field, _relativeParentFields, _fullParentFields,
					_schemaBuilder.BuildEntityFields(
						_entityDefinition, TypeModel.GetModelOf<TData>(),
						_relativeParentFields.Concat(new[] { field }),
						_fullParentFields.Concat(new[] { field }),
						_source
						), _source);

			private EntityField BuildReferencedField<TData>(IField field, EntityDefinition entityDefinition)
			{
				var foreignTableName = entityDefinition.TableName;
				var join = new EntityJoin(_source, new TableReference(foreignTableName), $"__join_{_joinCount++}");
				return ReferencedEntityField<TData>.Create(field, _relativeParentFields, _fullParentFields,
					_schemaBuilder.BuildEntityFields(
						entityDefinition, TypeModel.GetModelOf<TData>(),
						new IField[0],
						_fullParentFields.Concat(new[] { field }),
						join
						), _source);
			}

			void IFieldGenericExecutor.Execute<TField, TData>(IField field)
			{
				var storageSqlType = SqlTypeHelper.GetDataType(field.FieldDataType);
				var entityDefinitions = _schemaBuilder._entityDefinitions.Where(q => q.EntityType == field.FieldDataType)
						.ToArray();
				if (storageSqlType != null)
					EntityField = BuildValueField<TData>(field);
				else if (entityDefinitions.Length == 0)
					EntityField = BuildEmbeddedField<TData>(field);
				else
					//  todo: handle cases where there's multiple definitions for TData
					EntityField = BuildReferencedField<TData>(field, entityDefinitions.First());
			}
		}
	}
}
