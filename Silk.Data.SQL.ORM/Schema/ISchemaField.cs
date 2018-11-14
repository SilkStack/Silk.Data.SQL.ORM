namespace Silk.Data.SQL.ORM.Schema
{
	public interface ISchemaField
	{
		string FieldName { get; }
		Column Column { get; }
		IFieldExpressionFactory ExpressionFactory { get; }
		bool IsPrimaryKey { get; }
		PrimaryKeyGenerator PrimaryKeyGenerator { get; }
	}

	public class SqlPrimitiveSchemaField<TValue, TEntity> : ISchemaField
		where TEntity : class
	{
		public Column Column { get; }

		public IFieldExpressionFactory ExpressionFactory { get; }

		public bool IsPrimaryKey => PrimaryKeyGenerator != PrimaryKeyGenerator.NotPrimaryKey;

		public PrimaryKeyGenerator PrimaryKeyGenerator { get; }

		public string FieldName { get; }

		public SqlPrimitiveSchemaField(string fieldName, Column column, PrimaryKeyGenerator primaryKeyGenerator)
		{
			FieldName = fieldName;
			Column = column;
			PrimaryKeyGenerator = primaryKeyGenerator;
			ExpressionFactory = new SqlPrimitiveFieldExpressionFactory<TValue, TEntity>(this);
		}
	}
}
