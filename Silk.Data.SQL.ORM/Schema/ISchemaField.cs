using Silk.Data.Modelling;

namespace Silk.Data.SQL.ORM.Schema
{
	public interface ISchemaField
	{
		string AliasName { get; }
		string FieldName { get; }
		Column Column { get; }
		bool IsPrimaryKey { get; }
		PrimaryKeyGenerator PrimaryKeyGenerator { get; }
		ISchemaFieldReference FieldReference { get; }
		FieldType FieldType { get; }
		IFieldReference EntityFieldReference { get; }
		EntityFieldJoin Join { get; }
		System.Type DataType { get; }
		string[] ModelPath { get; }
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

		public string AliasName { get; }

		public EntityFieldJoin Join { get; }

		public System.Type DataType => typeof(TValue);

		public string[] ModelPath { get; }

		public SqlPrimitiveSchemaField(string fieldName, Column column, PrimaryKeyGenerator primaryKeyGenerator,
			IFieldReference entityFieldReference, EntityFieldJoin join, string[] modelPath,
			string aliasName)
		{
			FieldName = fieldName;
			Column = column;
			PrimaryKeyGenerator = primaryKeyGenerator;
			EntityFieldReference = entityFieldReference;
			FieldType = join != null ? FieldType.JoinedField : FieldType.StoredField;
			Join = join;
			ModelPath = modelPath;
			AliasName = aliasName;
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

		public string AliasName { get; }

		public EntityFieldJoin Join { get; }

		public System.Type DataType => typeof(TValue);

		public string[] ModelPath { get; }

		public EmbeddedObjectNullCheckSchemaField(string fieldName, string columnName,
			IFieldReference entityFieldReference, EntityFieldJoin join,
			IEntitySchemaAssemblage<TEntity> entitySchemaAssemblage,
			string[] modelPath, string aliasName)
		{
			if (join != null)
			{
				Join = join;
				FieldType = FieldType.JoinedField;
				Column = new Column(columnName, SqlDataType.Bit(), false, join.TableAlias);
			}
			else
			{
				Column = new Column(columnName, SqlDataType.Bit(), false, entitySchemaAssemblage.TableName);
				FieldType = FieldType.StoredField;
			}
			FieldName = fieldName;
			EntityFieldReference = entityFieldReference;
			ModelPath = modelPath;
			AliasName = aliasName;
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

		public string AliasName { get; }

		public EntityFieldJoin Join { get; }

		public System.Type DataType => typeof(TPrimaryKey);

		public string[] ModelPath { get; }

		public JoinedObjectSchemaField(string fieldName, string columnName,
			IFieldReference entityFieldReference, EntityFieldJoin join,
			IEntitySchemaAssemblage<TEntity> entitySchemaAssemblage,
			string[] modelPath, string aliasName)
		{
			if (join == null)
			{
				Column = new Column(columnName, SqlTypeHelper.GetDataType(typeof(TPrimaryKey)), true, entitySchemaAssemblage.TableName);
				FieldType = FieldType.ReferenceField;
			}
			else
			{
				Column = new Column(columnName, SqlTypeHelper.GetDataType(typeof(TPrimaryKey)), true, join.TableAlias);
				FieldType = FieldType.JoinedField;
			}
			Join = join;
			FieldName = fieldName;
			EntityFieldReference = entityFieldReference;
			ModelPath = modelPath;
			AliasName = aliasName;
		}
	}
}
