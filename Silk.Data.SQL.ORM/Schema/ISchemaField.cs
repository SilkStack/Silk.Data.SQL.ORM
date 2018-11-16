using Silk.Data.Modelling;

namespace Silk.Data.SQL.ORM.Schema
{
	public interface ISchemaField
	{
		string FieldName { get; }
		Column Column { get; }
		bool IsPrimaryKey { get; }
		PrimaryKeyGenerator PrimaryKeyGenerator { get; }
		ISchemaFieldReference FieldReference { get; }
	}

	public interface ISchemaField<TEntity> : ISchemaField
		where TEntity : class
	{
	}

	public class SqlPrimitiveSchemaField<TValue, TEntity> : ISchemaField<TEntity>
		where TEntity : class
	{
		public Column Column { get; }

		public bool IsPrimaryKey => PrimaryKeyGenerator != PrimaryKeyGenerator.NotPrimaryKey;

		public PrimaryKeyGenerator PrimaryKeyGenerator { get; }

		public string FieldName { get; }

		public ISchemaFieldReference FieldReference => throw new System.NotImplementedException();

		public IFieldReference EntityFieldReference { get; }

		public SqlPrimitiveSchemaField(string fieldName, Column column, PrimaryKeyGenerator primaryKeyGenerator,
			IFieldReference entityFieldReference)
		{
			FieldName = fieldName;
			Column = column;
			PrimaryKeyGenerator = primaryKeyGenerator;
			EntityFieldReference = entityFieldReference;
		}
	}

	public class EmbeddedObjectNullCheckSchemaField<TValue, TEntity> : ISchemaField<TEntity>
		where TEntity : class
	{
		public Column Column { get; }

		public bool IsPrimaryKey => false;

		public PrimaryKeyGenerator PrimaryKeyGenerator => PrimaryKeyGenerator.NotPrimaryKey;

		public string FieldName { get; }

		public ISchemaFieldReference FieldReference => throw new System.NotImplementedException();

		public IFieldReference EntityFieldReference { get; }

		public EmbeddedObjectNullCheckSchemaField(string fieldName, string columnName, IFieldReference entityFieldReference)
		{
			Column = new Column(columnName, SqlDataType.Bit(), false);
			FieldName = fieldName;
			EntityFieldReference = entityFieldReference;
		}
	}

	public class JoinedObjectSchemaField<TValue, TEntity> : ISchemaField<TEntity>
		where TEntity : class
	{
		public string FieldName => throw new System.NotImplementedException();

		public Column Column => throw new System.NotImplementedException();

		public bool IsPrimaryKey => throw new System.NotImplementedException();

		public PrimaryKeyGenerator PrimaryKeyGenerator => throw new System.NotImplementedException();

		public ISchemaFieldReference FieldReference => throw new System.NotImplementedException();
	}
}
