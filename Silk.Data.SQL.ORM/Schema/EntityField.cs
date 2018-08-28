using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping.Binding;
using Silk.Data.SQL.ORM.Queries;
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
		public abstract string[] ModelPath { get; }

		public bool IsPrimaryKey => PrimaryKeyGenerator != PrimaryKeyGenerator.NotPrimaryKey;

		public abstract Binding GetValueBinding();
	}

	public abstract class EntityField<TEntity> : EntityField
	{
		public abstract IValueReader GetValueReader(TEntity obj, Column column);
	}

	/// <summary>
	/// An entity field that stores type TValue.
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	public class EntityField<TValue, TEntity> : EntityField<TEntity>
	{
		private static TypeModel<TEntity> _entityModel = TypeModel.GetModelOf<TEntity>();

		public override Type DataType { get; } = typeof(TValue);
		public override Column[] Columns { get; }
		public override IPropertyField ModelField { get; }
		public override PrimaryKeyGenerator PrimaryKeyGenerator { get; }
		public override string[] ModelPath { get; }

		public EntityField(Column[] columns, IPropertyField modelField,
			PrimaryKeyGenerator primaryKeyGenerator, string[] modelPath)
		{
			Columns = columns;
			ModelField = modelField;
			PrimaryKeyGenerator = primaryKeyGenerator;
			ModelPath = modelPath;
		}

		public override IValueReader GetValueReader(TEntity obj, Column column)
		{
			return new ValueReader(
				new ObjectReadWriter(obj, _entityModel, typeof(TEntity)),
				ModelPath
				);
		}

		public override Binding GetValueBinding()
		{
			if (PrimaryKeyGenerator == PrimaryKeyGenerator.ServerGenerated)
			{
				return new CopyBinding<TValue>(new[] { "__PK_IDENTITY" }, ModelPath);
			}
			return new CopyBinding<TValue>(new[] { Columns[0].ColumnName }, ModelPath);
		}

		private class ValueReader : IValueReader
		{
			private readonly ObjectReadWriter _objectReadWriter;
			private readonly string[] _modelPath;

			public ValueReader(ObjectReadWriter objectReadWriter, string[] modelPath)
			{
				_objectReadWriter = objectReadWriter;
				_modelPath = modelPath;
			}

			public object Read()
			{
				return _objectReadWriter.ReadField<TValue>(_modelPath, 0);
			}
		}
	}
}
