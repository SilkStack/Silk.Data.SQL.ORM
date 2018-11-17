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
		FieldType FieldType { get; }
		IFieldReference EntityFieldReference { get; }
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

		public FieldType FieldType { get; }

		public SqlPrimitiveSchemaField(string fieldName, Column column, PrimaryKeyGenerator primaryKeyGenerator,
			IFieldReference entityFieldReference)
		{
			FieldName = fieldName;
			Column = column;
			PrimaryKeyGenerator = primaryKeyGenerator;
			EntityFieldReference = entityFieldReference;

			FieldType = column == null ? FieldType.JoinedField : FieldType.StoredField;
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

		public FieldType FieldType { get; }

		public EmbeddedObjectNullCheckSchemaField(string fieldName, string columnName, IFieldReference entityFieldReference)
		{
			if (columnName == null)
			{
				FieldType = FieldType.JoinedField;
			}
			else
			{
				Column = new Column(columnName, SqlDataType.Bit(), false);
				FieldType = FieldType.StoredField;
			}
			FieldName = fieldName;
			EntityFieldReference = entityFieldReference;
		}
	}

	public class JoinedObjectSchemaField<TValue, TEntity, TPrimaryKey> : ISchemaField<TEntity>
		where TEntity : class
		where TValue : class
	{
		public string FieldName { get; }

		public Column Column { get; }

		public bool IsPrimaryKey => false;

		public PrimaryKeyGenerator PrimaryKeyGenerator => PrimaryKeyGenerator.NotPrimaryKey;

		public ISchemaFieldReference FieldReference => throw new System.NotImplementedException();

		public IFieldReference EntityFieldReference { get; }

		public FieldType FieldType { get; }

		public JoinedObjectSchemaField(string fieldName, string columnName, IFieldReference entityFieldReference)
		{
			if (columnName != null)
			{
				Column = new Column(columnName, SqlTypeHelper.GetDataType(typeof(TPrimaryKey)), true);
				FieldType = FieldType.ReferenceField;
			}
			else
			{
				FieldType = FieldType.JoinedField;
			}
			FieldName = fieldName;
			EntityFieldReference = entityFieldReference;
		}
	}
}
