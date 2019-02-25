using System;
using System.Collections.Generic;
using System.Linq;
using Silk.Data.Modelling;
using Silk.Data.Modelling.GenericDispatch;
using Silk.Data.SQL.ORM.Queries;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class EntityField : IField
	{
		private static int _projectionAliasCounter;

		public string FieldName { get; }

		public string ProjectionAlias { get; }

		public bool CanRead { get; }

		public bool CanWrite { get; }

		public bool IsPrimaryKey { get; }

		public bool IsSeverGenerated { get; }

		public bool IsEnumerableType => false;

		public Type FieldDataType { get; }

		public Type FieldElementType => null;

		public Column Column { get; }

		public IReadOnlyList<EntityField> SubFields { get; }

		public IQueryReference Source { get; }

		public bool IsEntityLocalField { get; }

		protected EntityField(
			string fieldName, bool canRead, bool canWrite,
			Type fieldDataType, Column column,
			IEnumerable<EntityField> subFields = null,
			IQueryReference source = null,
			bool isPrimaryKey = false
			)
		{
			FieldName = fieldName;
			CanRead = canRead;
			CanWrite = canWrite;
			FieldDataType = fieldDataType;

			IsPrimaryKey = isPrimaryKey;
			Column = column;
			SubFields = subFields?.ToArray() ?? new EntityField[0];

			if (IsPrimaryKey && FieldDataType != typeof(Guid))
				IsSeverGenerated = true;

			Source = source;
			IsEntityLocalField = Source is TableReference;

			ProjectionAlias = $"__alias_{fieldName}_{_projectionAliasCounter++}";
		}

		public abstract void Dispatch(IFieldGenericExecutor executor);
	}

	public abstract class EntityField<TEntity> : EntityField
		where TEntity : class
	{
		protected EntityField(string fieldName, bool canRead, bool canWrite, Type fieldDataType,
			Column column, IEnumerable<EntityField> subFields = null, IQueryReference source = null,
			bool isPrimaryKey = false) :
			base(fieldName, canRead, canWrite, fieldDataType, column, subFields, source, isPrimaryKey)
		{
		}
	}

	public class ValueEntityField<T, TEntity> : EntityField<TEntity>
		where TEntity : class
	{
		public ValueEntityField(string fieldName, bool canRead, bool canWrite,
			Column column, IQueryReference source, bool isPrimaryKey) :
			base(fieldName, canRead, canWrite, typeof(T), column, null, source, isPrimaryKey)
		{
		}

		public override void Dispatch(IFieldGenericExecutor executor)
			=> executor.Execute<EntityField, T>(this);

		public static ValueEntityField<T, TEntity> Create(IField modelField, IEnumerable<IField> relativeParentFields,
			IEnumerable<IField> fullParentFields, IQueryReference source)
		{
			var columnNamePrefix = string.Join("_", relativeParentFields.Select(q => q.FieldName));
			if (!string.IsNullOrEmpty(columnNamePrefix))
				columnNamePrefix = $"{columnNamePrefix}_";

			return new ValueEntityField<T, TEntity>(modelField.FieldName, modelField.CanRead,
				modelField.CanWrite,
				//  value storage column
				new Column(
					$"{columnNamePrefix}{modelField.FieldName}", SqlTypeHelper.GetDataType(typeof(T)),
					SqlTypeHelper.TypeIsNullable(typeof(T))),
				source, modelField.FieldName == "Id" && relativeParentFields.Count() == 0);
		}
	}

	public class EmbeddedEntityField<T, TEntity> : EntityField<TEntity>
		where TEntity : class
	{
		public EmbeddedEntityField(string fieldName, bool canRead, bool canWrite,
			IEnumerable<EntityField> subFields, IQueryReference source) :
			base(fieldName, canRead, canWrite, typeof(T), null, subFields, source)
		{
		}

		public override void Dispatch(IFieldGenericExecutor executor)
			=> executor.Execute<EntityField, T>(this);

		public static EmbeddedEntityField<T, TEntity> Create(IField modelField, IEnumerable<IField> relativeParentFields,
			IEnumerable<IField> fullParentFields, IEnumerable<EntityField> subFields, IQueryReference source)
		{
			var columnNamePrefix = string.Join("_", relativeParentFields.Select(q => q.FieldName));
			if (!string.IsNullOrEmpty(columnNamePrefix))
				columnNamePrefix = $"{columnNamePrefix}_";

			return new EmbeddedEntityField<T, TEntity>(
				modelField.FieldName, modelField.CanRead, modelField.CanWrite,
				subFields, source
				);
		}
	}

	public class ReferencedEntityField<T, TEntity> : EntityField<TEntity>
		where TEntity : class
	{
		public ReferencedEntityField(string fieldName, bool canRead, bool canWrite,
			IEnumerable<EntityField> subFields, IQueryReference source) :
			base(fieldName, canRead, canWrite, typeof(T), null, subFields, source)
		{
		}

		public override void Dispatch(IFieldGenericExecutor executor)
			=> executor.Execute<EntityField, T>(this);

		public static ReferencedEntityField<T, TEntity> Create(IField modelField, IEnumerable<IField> relativeParentFields,
			IEnumerable<IField> fullParentFields, IEnumerable<EntityField> subFields, IQueryReference source)
		{
			//  only create the sub fields once please
			var subFieldsArray = subFields.ToArray();

			var primaryKeyFields = new List<EntityField>();
			var foreignKeyBuilder = new ForeignKeyEntityBuilder(
				modelField, relativeParentFields, fullParentFields, source
				);
			foreach (var subField in subFieldsArray.Where(q => q.IsPrimaryKey))
			{
				subField.Dispatch(foreignKeyBuilder);
				primaryKeyFields.Add(foreignKeyBuilder.EntityField);
			}

			return new ReferencedEntityField<T, TEntity>(
				modelField.FieldName, modelField.CanRead, modelField.CanWrite,
				primaryKeyFields.Concat(subFields), source
				);
		}

		private class ForeignKeyEntityBuilder : IFieldGenericExecutor
		{
			private readonly IField _modelField;
			private readonly IEnumerable<IField> _relativeParentFields;
			private readonly IEnumerable<IField> _fullParentFields;
			private readonly IQueryReference _source;

			public EntityField EntityField { get; private set; }

			public ForeignKeyEntityBuilder(
				IField modelField, IEnumerable<IField> relativeParentFields,
				IEnumerable<IField> fullParentFields, IQueryReference source
				)
			{
				_modelField = modelField;
				_relativeParentFields = relativeParentFields;
				_fullParentFields = fullParentFields;
				_source = source;
			}

			void IFieldGenericExecutor.Execute<TField, TData>(IField field)
			{
				var columnNamePrefix = string.Join("_", _relativeParentFields.Select(q => q.FieldName));
				if (!string.IsNullOrEmpty(columnNamePrefix))
					columnNamePrefix = $"{columnNamePrefix}_";

				EntityField = new ValueEntityField<TData, TEntity>(field.FieldName, field.CanRead,
					field.CanWrite,
					//  value storage column
					new Column(
						$"{columnNamePrefix}{_modelField.FieldName}_{field.FieldName}", SqlTypeHelper.GetDataType(typeof(T)),
						SqlTypeHelper.TypeIsNullable(typeof(T))),
						_source, false);
			}
		}
	}
}
