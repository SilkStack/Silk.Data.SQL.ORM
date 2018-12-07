﻿using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries.Expressions;

namespace Silk.Data.SQL.ORM.Schema
{
	public interface IFieldExpressionFactory
	{
		ColumnDefinitionExpression DefineColumn();
	}

	public interface IFieldExpressionFactory<TEntity> : IFieldExpressionFactory
		where TEntity : class
	{
		ValueExpression Value(TEntity entity, ObjectReadWriter entityReadWriter = null);
	}

	public interface IFieldExpressionFactory<TValue, TEntity> : IFieldExpressionFactory<TEntity>
		where TEntity : class
	{
	}

	public class SqlPrimitiveFieldExpressionFactory<TValue, TEntity> : IFieldExpressionFactory<TValue, TEntity>
		where TEntity : class
	{
		private readonly static TypeModel<TEntity> _entityTypeModel = TypeModel.GetModelOf<TEntity>();
		private readonly SqlPrimitiveSchemaField<TValue, TEntity> _field;

		public SqlPrimitiveFieldExpressionFactory(SqlPrimitiveSchemaField<TValue, TEntity> field)
		{
			_field = field;
		}

		public ColumnDefinitionExpression DefineColumn() =>
			QueryExpression.DefineColumn(_field.Column.ColumnName, _field.Column.DataType, _field.Column.IsNullable,
				_field.PrimaryKeyGenerator == PrimaryKeyGenerator.ServerGenerated, _field.IsPrimaryKey);

		public ValueExpression Value(TEntity entity, ObjectReadWriter entityReadWriter = null)
		{
			if (entityReadWriter == null)
				entityReadWriter = new ObjectReadWriter(entity, _entityTypeModel, typeof(TEntity));

			entityReadWriter.WriteField(_entityTypeModel.Root, entity);
			var value = entityReadWriter.ReadField<TValue>(_field.EntityFieldReference);
			return ORMQueryExpressions.Value(value);
		}
	}

	public class EmbeddedObjectNullCheckExpressionFactory<TValue, TEntity> : IFieldExpressionFactory<TEntity>
		where TEntity : class
	{
		private readonly static TypeModel<TEntity> _entityTypeModel = TypeModel.GetModelOf<TEntity>();
		private readonly EmbeddedObjectNullCheckSchemaField<TValue, TEntity> _field;

		public EmbeddedObjectNullCheckExpressionFactory(EmbeddedObjectNullCheckSchemaField<TValue, TEntity> field)
		{
			_field = field;
		}

		public ColumnDefinitionExpression DefineColumn() =>
			QueryExpression.DefineColumn(_field.Column.ColumnName, _field.Column.DataType, _field.Column.IsNullable);

		public ValueExpression Value(TEntity entity, ObjectReadWriter entityReadWriter = null)
		{
			if (entityReadWriter == null)
				entityReadWriter = new ObjectReadWriter(entity, _entityTypeModel, typeof(TEntity));

			entityReadWriter.WriteField(_entityTypeModel.Root, entity);
			var value = entityReadWriter.ReadField<TValue>(_field.EntityFieldReference);
			return ORMQueryExpressions.Value(value != null);
		}
	}

	public class JoinedObjectExpressionFactory<TValue, TEntity, TPrimaryKey> : IFieldExpressionFactory<TEntity>
		where TEntity : class
		where TValue : class
	{
		private readonly static TypeModel<TEntity> _entityTypeModel = TypeModel.GetModelOf<TEntity>();
		private readonly JoinedObjectSchemaField<TValue, TEntity, TPrimaryKey> _field;
		private readonly IFieldReference _pkFieldReference;

		public JoinedObjectExpressionFactory(JoinedObjectSchemaField<TValue, TEntity, TPrimaryKey> field,
			IFieldReference primaryKeyFieldReference)
		{
			_field = field;
			_pkFieldReference = primaryKeyFieldReference;
		}

		public ColumnDefinitionExpression DefineColumn() =>
			QueryExpression.DefineColumn(_field.Column.ColumnName, _field.Column.DataType, _field.Column.IsNullable);

		public ValueExpression Value(TEntity entity, ObjectReadWriter entityReadWriter = null)
		{
			if (entityReadWriter == null)
				entityReadWriter = new ObjectReadWriter(entity, _entityTypeModel, typeof(TEntity));

			entityReadWriter.WriteField(_entityTypeModel.Root, entity);

			var relatedEntity = entityReadWriter.ReadField<TValue>(_field.EntityFieldReference);
			if (relatedEntity == null)
				return ORMQueryExpressions.Value(null);
			var value = entityReadWriter.ReadField<TPrimaryKey>(_pkFieldReference);
			return ORMQueryExpressions.Value(value);
		}
	}
}
