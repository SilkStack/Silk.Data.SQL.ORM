using Silk.Data.Modelling;
using CoreBinding = Silk.Data.Modelling.Mapping.Binding;
using Silk.Data.SQL.ORM.Queries;
using System;
using System.Linq;

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
		public abstract KeyType KeyType { get; }
		public abstract ForeignKey[] ForeignKeys { get; }

		public bool IsPrimaryKey => PrimaryKeyGenerator != PrimaryKeyGenerator.NotPrimaryKey;

		public abstract CoreBinding.Binding GetValueBinding();

		public abstract ForeignKey BuildForeignKey(string propertyPathPrefix, string[] modelPath);

		public abstract ProjectionField BuildProjectionField(string sourceName, string fieldName, string aliasName, string[] modelPath);
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
		public override KeyType KeyType { get; }
		public override ForeignKey[] ForeignKeys { get; }

		public EntityField(Column[] columns, IPropertyField modelField,
			PrimaryKeyGenerator primaryKeyGenerator, string[] modelPath)
		{
			Columns = columns;
			ModelField = modelField;
			PrimaryKeyGenerator = primaryKeyGenerator;
			ModelPath = modelPath;
			KeyType = KeyType.None;
		}

		public EntityField(IPropertyField modelField, string[] modelPath, KeyType keyType,
			ForeignKey[] foreignKeys)
		{
			Columns = foreignKeys.Select(q => q.LocalColumn).ToArray();
			ModelField = modelField;
			PrimaryKeyGenerator = PrimaryKeyGenerator.NotPrimaryKey;
			ModelPath = modelPath;
			ForeignKeys = foreignKeys;
			KeyType = keyType;
		}

		public override IValueReader GetValueReader(TEntity obj, Column column)
		{
			var objectReadWriter = new ObjectReadWriter(obj, _entityModel, typeof(TEntity));

			//  if EntityField represents a complex type read a true/false value representing null/not null
			if (!SqlTypeHelper.IsSqlPrimitiveType(DataType))
			{
				if (KeyType == KeyType.None)
				{
					return new NullBoolReader(objectReadWriter, ModelPath);
				}
				else
				{
					var foreignKey = ForeignKeys.First(q => q.LocalColumn == column);
					return foreignKey.CreateValueReader(objectReadWriter);
				}
			}

			return new ValueReader(objectReadWriter, ModelPath);
		}

		public override CoreBinding.Binding GetValueBinding()
		{
			if (PrimaryKeyGenerator == PrimaryKeyGenerator.ServerGenerated)
			{
				return new CoreBinding.CopyBinding<TValue>(new[] { "__PK_IDENTITY" }, ModelPath);
			}
			return new CoreBinding.CopyBinding<TValue>(new[] { Columns[0].ColumnName }, ModelPath);
		}

		public override ForeignKey BuildForeignKey(string propertyPathPrefix, string[] modelPath)
		{
			var column = Columns[0];
			return new ForeignKey<TEntity, TValue>(new Column(
					$"FK_{propertyPathPrefix}_{column.ColumnName}", column.DataType, true
					), column, modelPath.Concat(ModelPath).ToArray());
		}

		public override ProjectionField BuildProjectionField(string sourceName, string fieldName, string aliasName, string[] modelPath)
		{
			return new ProjectionField<TValue>(sourceName, fieldName, aliasName, modelPath);
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

		private class NullBoolReader : IValueReader
		{
			private readonly ObjectReadWriter _objectReadWriter;
			private readonly string[] _modelPath;

			public NullBoolReader(ObjectReadWriter objectReadWriter, string[] modelPath)
			{
				_objectReadWriter = objectReadWriter;
				_modelPath = modelPath;
			}

			public object Read()
			{
				var value = _objectReadWriter.ReadField<TValue>(_modelPath, 0);
				return value != null;
			}
		}
	}
}
