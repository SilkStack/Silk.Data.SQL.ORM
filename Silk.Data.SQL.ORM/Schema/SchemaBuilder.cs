﻿using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.Analysis.CandidateSources;
using Silk.Data.Modelling.Analysis.Rules;
using Silk.Data.Modelling.GenericDispatch;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Modelling;
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
	public class SchemaBuilder : IFieldBuilder
	{
		private static int _joinCount = 1;

		public List<IIntersectCandidateSource<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>>
			ClassToEntityCandidateSources { get; } = new List<IIntersectCandidateSource<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>>();

		public List<IIntersectCandidateSource<EntityModel, EntityField, TypeModel, PropertyInfoField>>
			EntityToTypeCandidateSources { get; } = new List<IIntersectCandidateSource<EntityModel, EntityField, TypeModel, PropertyInfoField>>();

		public List<IIntersectionRule<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>>
			ClassToEntityRules { get; } = new List<IIntersectionRule<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>>();

		public List<IIntersectionRule<EntityModel, EntityField, TypeModel, PropertyInfoField>>
			EntityToTypeRules { get; } = new List<IIntersectionRule<EntityModel, EntityField, TypeModel, PropertyInfoField>>();

		protected List<EntityDefinition> EntityDefinitions
			= new List<EntityDefinition>();

		protected Dictionary<MethodInfo, IMethodCallConverter> MethodCallConverters
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

			EntityToTypeCandidateSources.AddRange(
				GetDefaultCandidateSources<EntityModel, EntityField, TypeModel, PropertyInfoField>()
				);
			ClassToEntityCandidateSources.AddRange(
				GetDefaultCandidateSources<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>()
				);

			EntityToTypeRules.AddRange(
				GetDefaultRules<EntityModel, EntityField, TypeModel, PropertyInfoField>()
				);
			ClassToEntityRules.AddRange(
				GetDefaultRules<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>()
				);
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

		protected virtual IEnumerable<IIntersectionRule<TLeftModel, TLeftField, TRightModel, TRightField>>
			GetDefaultRules<TLeftModel, TLeftField, TRightModel, TRightField>()
			where TLeftField : class, IField
			where TRightField : class, IField
			where TLeftModel : IModel<TLeftField>
			where TRightModel : IModel<TRightField>
		{
			yield return new SameDataTypeRule<TLeftModel, TLeftField, TRightModel, TRightField>();
			yield return new BothNumericTypesRule<TLeftModel, TLeftField, TRightModel, TRightField>();
			yield return new ConvertableWithToStringRule<TLeftModel, TLeftField, TRightModel, TRightField>();
			yield return new ExplicitCastRule<TLeftModel, TLeftField, TRightModel, TRightField>();
			yield return new ConvertableWithTryParse<TLeftModel, TLeftField, TRightModel, TRightField>();
		}

		protected virtual IEnumerable<IIntersectCandidateSource<TLeftModel, TLeftField, TRightModel, TRightField>>
			GetDefaultCandidateSources<TLeftModel, TLeftField, TRightModel, TRightField>()
			where TLeftField : class, IField
			where TRightField : class, IField
			where TLeftModel : IModel<TLeftField>
			where TRightModel : IModel<TRightField>
		{
			yield return new ExactPathMatchCandidateSource<TLeftModel, TLeftField, TRightModel, TRightField>();
			yield return new FlattenedNameMatchCandidateSource<TLeftModel, TLeftField, TRightModel, TRightField>();
		}

		public SchemaBuilder AddMethodConverter(MethodInfo methodInfo, IMethodCallConverter methodCallConverter)
		{
			MethodCallConverters.Add(methodInfo, methodCallConverter);
			return this;
		}

		public SchemaBuilder AddTypeConverters(IEnumerable<ITypeConverter> typeConverters)
		{
			ClassToEntityRules.Add(
				new TypeConverterRule<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>(typeConverters)
				);
			EntityToTypeRules.Add(
				new TypeConverterRule<EntityModel, EntityField, TypeModel, PropertyInfoField>(typeConverters)
				);
			return this;
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
			var definition = GetDefinition<T>();

			if (definition == null)
			{
				definition = new EntityDefinition<T>();
				EntityDefinitions.Add(definition);
			}

			configure?.Invoke(definition);
			return this;
		}

		/// <summary>
		/// Get all the entity definitions for the provided entity type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public EntityDefinition<T> GetDefinition<T>()
			where T : class
			=> EntityDefinitions.OfType<EntityDefinition<T>>()
				.FirstOrDefault();

		/// <summary>
		/// Build the schema.
		/// </summary>
		/// <returns></returns>
		public virtual Schema Build()
		{
			return new Schema(
				BuildEntityModels(),
				MethodCallConverters
				);
		}

		protected virtual IEnumerable<EntityModel> BuildEntityModels()
		{
			var classToEntityAnalyzer = new ClassToEntityIntersectionAnalyzer(
				ClassToEntityCandidateSources,
				ClassToEntityRules
				);
			var modelToTypeAnalyzer = new DefaultIntersectionAnalyzer<EntityModel, EntityField, TypeModel, PropertyInfoField>(
				EntityToTypeCandidateSources,
				EntityToTypeRules
				);
			foreach (var entityDefinition in EntityDefinitions)
			{
				yield return BuildEntityModel(classToEntityAnalyzer, modelToTypeAnalyzer, entityDefinition);
			}
		}

		protected virtual EntityModel BuildEntityModel(
			ClassToEntityIntersectionAnalyzer classToEntityAnalyzer,
			IIntersectionAnalyzer<EntityModel, EntityField, TypeModel, PropertyInfoField> modelToTypeAnalyzer,
			EntityDefinition entityDefinition)
		{
			_joinCount = 1;
			var modelFields = entityDefinition.BuildEntityFields(this, entityDefinition, entityDefinition.TypeModel)
				.ToArray();
			var indexes = entityDefinition.IndexBuilders.Select(q => q.Build(modelFields));
			return entityDefinition.BuildModel(
				classToEntityAnalyzer, modelToTypeAnalyzer,
				modelFields, indexes
				);
		}

		public virtual IEnumerable<EntityField> BuildEntityFields<TEntity>(
			EntityDefinition rootEntityDefinition,
			EntityDefinition entityDefinition,
			TypeModel typeModel,
			IEnumerable<IField> relativeParentFields = null,
			IEnumerable<IField> fullParentFields = null,
			IQueryReference source = null
			)
			where TEntity : class
		{
			if (source == null)
				source = new TableReference(entityDefinition.TableName);

			var builder = new EntityFieldBuilder<TEntity>(rootEntityDefinition, entityDefinition, this,
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

		private class EntityFieldBuilder<TEntity> : IFieldGenericExecutor
			where TEntity : class
		{
			private readonly EntityDefinition _rootEntityDefinition;
			private readonly EntityDefinition _entityDefinition;
			private readonly SchemaBuilder _schemaBuilder;
			private readonly IEnumerable<IField> _relativeParentFields;
			private readonly IEnumerable<IField> _fullParentFields;
			private readonly IQueryReference _source;

			public EntityField<TEntity> EntityField { get; private set; }

			public EntityFieldBuilder(EntityDefinition rootEntityDefinition,
				EntityDefinition entityDefinition, SchemaBuilder schemaBuilder,
				IEnumerable<IField> relativeParentFields, IEnumerable<IField> fullParentFields,
				IQueryReference source)
			{
				_rootEntityDefinition = rootEntityDefinition;
				_entityDefinition = entityDefinition;
				_schemaBuilder = schemaBuilder;
				_relativeParentFields = relativeParentFields;
				_fullParentFields = fullParentFields;
				_source = source;
			}

			private EntityField<TEntity> BuildValueField<TData>(IField field)
				=> ValueEntityField<TData, TEntity>.Create(field, _relativeParentFields, _fullParentFields, _source);

			private EntityField<TEntity> BuildEmbeddedField<TData>(IField field)
				=> EmbeddedEntityField<TData, TEntity>.Create(field, _relativeParentFields, _fullParentFields,
					_rootEntityDefinition.BuildEntityFields(_schemaBuilder,
						_entityDefinition, TypeModel.GetModelOf<TData>(),
						_relativeParentFields.Concat(new[] { field }),
						_fullParentFields.Concat(new[] { field }),
						_source
						), _source);

			private EntityField<TEntity> BuildReferencedField<TData>(IField field, EntityDefinition entityDefinition)
			{
				var foreignTableName = entityDefinition.TableName;
				var join = new EntityJoin(_source, new TableReference(foreignTableName), $"__join_{_joinCount++}");
				var subFields = _rootEntityDefinition.BuildEntityFields(_schemaBuilder,
					entityDefinition, TypeModel.GetModelOf<TData>(),
					new IField[0],
					_fullParentFields.Concat(new[] { field }),
					join
					).ToArray();
				var referencedEntityField = ReferencedEntityField<TData, TEntity>.Create(field, _relativeParentFields, _fullParentFields,
					subFields, _source);

				var joinPairs = new List<JoinColumnPair>();
				foreach (var pkField in subFields.Where(q => !q.IsEntityLocalField && q.IsPrimaryKey))
				{
					var pairedField = referencedEntityField.SubFields.First(
						q => q.FieldName == pkField.FieldName && !ReferenceEquals(q, pkField)
						);
					joinPairs.Add(
						new JoinColumnPair(pairedField.Column.Name, pkField.Column.Name)
						);
				}
				join.SetJoinColumns(joinPairs);

				return referencedEntityField;
			}

			void IFieldGenericExecutor.Execute<TField, TData>(IField field)
			{
				var storageSqlType = SqlTypeHelper.GetDataType(field.FieldDataType);
				var entityDefinitions = _schemaBuilder.EntityDefinitions.Where(q => q.EntityType == field.FieldDataType)
						.ToArray();
				if (storageSqlType != null)
					EntityField = BuildValueField<TData>(field);
				else if (entityDefinitions.Length == 0)
					EntityField = BuildEmbeddedField<TData>(field);
				else
					EntityField = BuildReferencedField<TData>(field, entityDefinitions.First());
			}
		}
	}
}
