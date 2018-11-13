using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.ORM.Schema
{
	public interface IFieldExpressionFactory
	{
		ColumnDefinitionExpression DefineColumn();
	}

	public interface IFieldExpressionFactory<TValue, TEntity> : IFieldExpressionFactory
	{
	}

	public class SqlPrimitiveFieldExpressionFactory<TValue, TEntity> : IFieldExpressionFactory<TValue, TEntity>
		where TEntity : class
	{
		private readonly SqlPrimitiveSchemaField<TValue, TEntity> _field;

		public SqlPrimitiveFieldExpressionFactory(SqlPrimitiveSchemaField<TValue, TEntity> field)
		{
			_field = field;
		}

		public ColumnDefinitionExpression DefineColumn() =>
			QueryExpression.DefineColumn(_field.Column.ColumnName, _field.Column.DataType, _field.Column.IsNullable,
				_field.PrimaryKeyGenerator == PrimaryKeyGenerator.ServerGenerated, _field.IsPrimaryKey);
	}
}
