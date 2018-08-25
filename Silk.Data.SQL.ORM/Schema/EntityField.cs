using Silk.Data.Modelling;
using System;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// An entity field.
	/// </summary>
	public abstract class EntityField : ITableField
	{
		public abstract Type DataType { get; }
		public abstract Column[] Columns { get; }
		public abstract IPropertyField ModelField { get; }
		public abstract PrimaryKeyGenerator PrimaryKeyGenerator { get; }

		public bool IsPrimaryKey => PrimaryKeyGenerator != PrimaryKeyGenerator.NotPrimaryKey;
	}

	/// <summary>
	/// An entity field that stores type T.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class EntityField<T> : EntityField
	{
		public override Type DataType { get; } = typeof(T);
		public override Column[] Columns { get; }
		public override IPropertyField ModelField { get; }
		public override PrimaryKeyGenerator PrimaryKeyGenerator { get; }

		public EntityField(Column[] columns, IPropertyField modelField,
			PrimaryKeyGenerator primaryKeyGenerator)
		{
			Columns = columns;
			ModelField = modelField;
			PrimaryKeyGenerator = primaryKeyGenerator;
		}
	}
}
