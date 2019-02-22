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
		public string FieldName { get; }

		public bool CanRead { get; }

		public bool CanWrite { get; }

		public bool IsPrimaryKey { get; }

		public bool IsSeverGenerated { get; }

		public bool IsEnumerableType => false;

		public Type FieldDataType { get; }

		public Type FieldElementType => null;

		public IReadOnlyList<Column> Columns { get; }

		public IReadOnlyList<EntityField> SubFields { get; }

		public IQueryReference Source { get; }

		protected EntityField(
			string fieldName, bool canRead, bool canWrite,
			Type fieldDataType, IEnumerable<Column> columns,
			IEnumerable<EntityField> subFields = null,
			IQueryReference source = null
			)
		{
			FieldName = fieldName;
			CanRead = canRead;
			CanWrite = canWrite;
			FieldDataType = fieldDataType;

			IsPrimaryKey = FieldName == "Id"; //  todo: temporary until it's provided from the entity configuration
			Columns = columns?.ToArray() ?? new Column[0];
			SubFields = subFields?.ToArray() ?? new EntityField[0];

			if (IsPrimaryKey && FieldDataType != typeof(Guid))
				IsSeverGenerated = true;

			Source = source;
		}

		public abstract void Dispatch(IFieldGenericExecutor executor);
	}

	public class ValueEntityField<T> : EntityField
	{
		public ValueEntityField(string fieldName, bool canRead, bool canWrite,
			Column column, IQueryReference source) :
			base(fieldName, canRead, canWrite, typeof(T), new[] { column }, null, source)
		{
		}

		public override void Dispatch(IFieldGenericExecutor executor)
			=> executor.Execute<EntityField, T>(this);

		public static ValueEntityField<T> Create(IField modelField, IEnumerable<IField> relativeParentFields,
			IEnumerable<IField> fullParentFields, IQueryReference source)
		{
			var columnNamePrefix = string.Join("_", relativeParentFields.Select(q => q.FieldName));
			if (!string.IsNullOrEmpty(columnNamePrefix))
				columnNamePrefix = $"{columnNamePrefix}_";

			return new ValueEntityField<T>(modelField.FieldName, modelField.CanRead,
				modelField.CanWrite,
				//  value storage column
				new Column(
					$"{columnNamePrefix}{modelField.FieldName}", SqlTypeHelper.GetDataType(typeof(T)),
					SqlTypeHelper.TypeIsNullable(typeof(T))),
				source);
		}
	}

	public class EmbeddedEntityField<T> : EntityField
	{
		public EmbeddedEntityField(string fieldName, bool canRead, bool canWrite,
			Column column, IEnumerable<EntityField> subFields, IQueryReference source) :
			base(fieldName, canRead, canWrite, typeof(T), new[] { column }, subFields, source)
		{
		}

		public override void Dispatch(IFieldGenericExecutor executor)
			=> executor.Execute<EntityField, T>(this);

		public static EmbeddedEntityField<T> Create(IField modelField, IEnumerable<IField> relativeParentFields,
			IEnumerable<IField> fullParentFields, IEnumerable<EntityField> subFields, IQueryReference source)
		{
			var columnNamePrefix = string.Join("_", relativeParentFields.Select(q => q.FieldName));
			if (!string.IsNullOrEmpty(columnNamePrefix))
				columnNamePrefix = $"{columnNamePrefix}_";

			return new EmbeddedEntityField<T>(
				modelField.FieldName, modelField.CanRead, modelField.CanWrite,
				//  null check column
				new Column(
					$"{columnNamePrefix}{modelField.FieldName}",
					SqlDataType.Bit(), false
					), subFields, source
				);
		}
	}

	public class ReferencedEntityField<T> : EntityField
	{
		public ReferencedEntityField(string fieldName, bool canRead, bool canWrite,
			IEnumerable<Column> columns, IEnumerable<EntityField> subFields, IQueryReference source) :
			base(fieldName, canRead, canWrite, typeof(T), columns, subFields, source)
		{
		}

		public override void Dispatch(IFieldGenericExecutor executor)
			=> executor.Execute<EntityField, T>(this);

		public static ReferencedEntityField<T> Create(IField modelField, IEnumerable<IField> relativeParentFields,
			IEnumerable<IField> fullParentFields, IEnumerable<EntityField> subFields, IQueryReference source)
		{
			//  only create the sub fields once please
			var subFieldsArray = subFields.ToArray();

			var columnNamePrefix = string.Join("_", relativeParentFields.Select(q => q.FieldName));
			if (!string.IsNullOrEmpty(columnNamePrefix))
				columnNamePrefix = $"{columnNamePrefix}_";

			var primaryKeyColumns = new List<Column>();
			foreach (var subField in subFieldsArray.Where(q => q.IsPrimaryKey))
			{
				primaryKeyColumns.Add(new Column(
					$"{columnNamePrefix}{modelField.FieldName}_{subField.FieldName}",
					SqlTypeHelper.GetDataType(subField.FieldDataType),
					true
					));
			}

			return new ReferencedEntityField<T>(
				modelField.FieldName, modelField.CanRead, modelField.CanWrite,
				primaryKeyColumns, subFields, source
				);
		}
	}
}
