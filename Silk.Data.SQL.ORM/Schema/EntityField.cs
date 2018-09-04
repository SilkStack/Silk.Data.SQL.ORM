﻿using Silk.Data.Modelling;
using CoreBinding = Silk.Data.Modelling.Mapping.Binding;
using Silk.Data.SQL.ORM.Queries;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// An entity field.
	/// </summary>
	public interface IEntityField : ITableField
	{
		Type DataType { get; }
		IPropertyField ModelField { get; }
		string[] ModelPath { get; }
		KeyType KeyType { get; }
		ForeignKey[] ForeignKeys { get; }

		CoreBinding.Binding GetValueBinding();

		ForeignKey BuildForeignKey(string propertyPathPrefix, string[] modelPath);

		ProjectionField BuildProjectionField(string sourceName, string fieldName,
			string aliasName, string[] modelPath, EntityFieldJoin join);
	}

	public interface IEntityFieldOfValue<TValue> : IEntityField
	{
	}

	public interface IEntityFieldOfEntity<TEntity> : IEntityField
	{
		FieldAssignment GetFieldValuePair(TEntity obj);
	}

	/// <summary>
	/// An entity field that stores type TValue.
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	public class EntityField<TValue, TEntity> : IEntityFieldOfValue<TValue>, IEntityFieldOfEntity<TEntity>
	{
		private readonly static TypeModel<TEntity> _entityModel = TypeModel.GetModelOf<TEntity>();

		public Type DataType { get; } = typeof(TValue);
		public Column[] Columns { get; }
		public IPropertyField ModelField { get; }
		public PrimaryKeyGenerator PrimaryKeyGenerator { get; }
		public string[] ModelPath { get; }
		public KeyType KeyType { get; }
		public ForeignKey[] ForeignKeys { get; }
		public bool IsPrimaryKey => PrimaryKeyGenerator != PrimaryKeyGenerator.NotPrimaryKey;

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

		public CoreBinding.Binding GetValueBinding()
		{
			if (PrimaryKeyGenerator == PrimaryKeyGenerator.ServerGenerated)
			{
				return new CoreBinding.CopyBinding<TValue>(new[] { "__PK_IDENTITY" }, ModelPath);
			}
			return new CoreBinding.CopyBinding<TValue>(new[] { Columns[0].ColumnName }, ModelPath);
		}

		public ForeignKey BuildForeignKey(string propertyPathPrefix, string[] modelPath)
		{
			var column = Columns[0];
			return new ForeignKey<TEntity, TValue>(new Column(
					$"FK_{propertyPathPrefix}_{column.ColumnName}", column.DataType, true
					), column, modelPath.Concat(ModelPath).ToArray());
		}

		public ProjectionField BuildProjectionField(string sourceName, string fieldName,
			string aliasName, string[] modelPath, EntityFieldJoin join)
		{
			return new ProjectionField<TValue>(sourceName, fieldName, aliasName, modelPath, join);
		}

		public FieldAssignment GetFieldValuePair(TEntity obj)
		{
			var objectReadWriter = new ObjectReadWriter(obj, _entityModel, typeof(TEntity));
			return new FieldValueAssignment<TValue>(this, new ValueReader(objectReadWriter, ModelPath));
		}

		private class ValueReader : IValueReader<TValue>
		{
			private readonly ObjectReadWriter _objectReadWriter;
			private readonly string[] _modelPath;

			public ValueReader(ObjectReadWriter objectReadWriter, string[] modelPath)
			{
				_objectReadWriter = objectReadWriter;
				_modelPath = modelPath;
			}

			TValue IValueReader<TValue>.Read()
			{
				return _objectReadWriter.ReadField<TValue>(_modelPath, 0);
			}
		}
	}
}
