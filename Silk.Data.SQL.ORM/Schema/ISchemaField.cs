namespace Silk.Data.SQL.ORM.Schema
{
	public interface ISchemaField
	{
		Column Column { get; }
		IFieldExpressionFactory ExpressionFactory { get; }
	}

	public class SqlPrimitiveSchemaField<TValue, TEntity> : ISchemaField
		where TEntity : class
	{
		public Column Column { get; }

		public IFieldExpressionFactory ExpressionFactory { get; }

		public SqlPrimitiveSchemaField(Column column)
		{
			Column = column;
			ExpressionFactory = new SqlPrimitiveFieldExpressionFactory<TValue, TEntity>(this);
		}
	}
}
