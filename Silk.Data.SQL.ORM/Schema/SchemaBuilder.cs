using Silk.Data.Modelling;
using Silk.Data.Modelling.GenericDispatch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Used to configure and build a schema.
	/// </summary>
	public class SchemaBuilder
	{
		private readonly List<EntityDefinition> _entityDefinitions
			= new List<EntityDefinition>();

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
				BuildEntityModels()
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
			return entityDefinition.BuildModel(
				BuildEntityFields(entityDefinition, entityDefinition.TypeModel)
				);
		}

		protected virtual IEnumerable<EntityField> BuildEntityFields(
			EntityDefinition entityDefinition,
			TypeModel typeModel,
			IEnumerable<IField> relativeParentFields = null,
			IEnumerable<IField> fullParentFields = null
			)
		{
			var builder = new EntityFieldBuilder(entityDefinition, this,
				relativeParentFields ?? new IField[0],
				fullParentFields ?? new IField[0]);
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
			private readonly EntityDefinition _entityDefinition;
			private readonly SchemaBuilder _schemaBuilder;
			private readonly IEnumerable<IField> _relativeParentFields;
			private readonly IEnumerable<IField> _fullParentFields;

			public EntityField EntityField { get; private set; }

			public EntityFieldBuilder(EntityDefinition entityDefinition, SchemaBuilder schemaBuilder,
				IEnumerable<IField> relativeParentFields, IEnumerable<IField> fullParentFields)
			{
				_entityDefinition = entityDefinition;
				_schemaBuilder = schemaBuilder;
				_relativeParentFields = relativeParentFields;
				_fullParentFields = fullParentFields;
			}

			private EntityField BuildValueField<TData>(IField field)
				=> ValueEntityField<TData>.Create(field, _relativeParentFields, _fullParentFields);

			private EntityField BuildEmbeddedField<TData>(IField field)
				=> EmbeddedEntityField<TData>.Create(field, _relativeParentFields, _fullParentFields,
					_schemaBuilder.BuildEntityFields(
						_entityDefinition, TypeModel.GetModelOf<TData>(),
						_relativeParentFields.Concat(new[] { field }),
						_fullParentFields.Concat(new[] { field })
						));

			private EntityField BuildReferencedField<TData>(IField field, EntityDefinition entityDefinition)
				=> ReferencedEntityField<TData>.Create(field, _relativeParentFields, _fullParentFields,
					_schemaBuilder.BuildEntityFields(
						entityDefinition, TypeModel.GetModelOf<TData>(),
						new IField[0],
						_fullParentFields.Concat(new[] { field })
						));

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
