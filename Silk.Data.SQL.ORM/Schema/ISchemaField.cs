using Silk.Data.Modelling;

namespace Silk.Data.SQL.ORM.Schema
{
	public interface ISchemaField
	{
		string FieldName { get; }
		Column Column { get; }
		IFieldExpressionFactory ExpressionFactory { get; }
		bool IsPrimaryKey { get; }
		PrimaryKeyGenerator PrimaryKeyGenerator { get; }
		ISchemaFieldReference FieldReference { get; }
	}

	public interface ISchemaField<TEntity> : ISchemaField
		where TEntity : class
	{
		new IFieldExpressionFactory<TEntity> ExpressionFactory { get; }
	}

	public class SqlPrimitiveSchemaField<TValue, TEntity> : ISchemaField<TEntity>
		where TEntity : class
	{
		public Column Column { get; }

		public bool IsPrimaryKey => PrimaryKeyGenerator != PrimaryKeyGenerator.NotPrimaryKey;

		public PrimaryKeyGenerator PrimaryKeyGenerator { get; }

		public string FieldName { get; }

		public ISchemaFieldReference FieldReference => throw new System.NotImplementedException();

		public IFieldExpressionFactory<TEntity> ExpressionFactory { get; }
		IFieldExpressionFactory ISchemaField.ExpressionFactory => ExpressionFactory;

		public IFieldReference EntityFieldReference { get; }

		public SqlPrimitiveSchemaField(string fieldName, Column column, PrimaryKeyGenerator primaryKeyGenerator,
			IFieldReference entityFieldReference)
		{
			FieldName = fieldName;
			Column = column;
			PrimaryKeyGenerator = primaryKeyGenerator;
			ExpressionFactory = new SqlPrimitiveFieldExpressionFactory<TValue, TEntity>(this);
			EntityFieldReference = entityFieldReference;
		}
	}
}
