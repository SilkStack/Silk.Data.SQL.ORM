using System;
using Silk.Data.Modelling;
using Silk.Data.Modelling.GenericDispatch;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class SchemaField : IField
	{
		public string AliasName => throw new NotImplementedException();

		public string FieldName => throw new NotImplementedException();

		public Column Column => throw new NotImplementedException();

		public bool IsPrimaryKey => throw new NotImplementedException();

		public PrimaryKeyGenerator PrimaryKeyGenerator => throw new NotImplementedException();

		public ISchemaFieldReference SchemaFieldReference => throw new NotImplementedException();

		public FieldType FieldType => throw new NotImplementedException();

		public EntityFieldJoin Join => throw new NotImplementedException();

		public Type DataType => throw new NotImplementedException();

		public string[] ModelPath => throw new NotImplementedException();

		public bool CanRead => throw new NotImplementedException();

		public bool CanWrite => throw new NotImplementedException();

		public bool IsEnumerableType => throw new NotImplementedException();

		public Type FieldDataType => throw new NotImplementedException();

		public Type FieldElementType => throw new NotImplementedException();

		public void Dispatch(IFieldGenericExecutor executor)
		{
			throw new NotImplementedException();
		}
	}

	public abstract class SchemaField<TEntity> : SchemaField
		where TEntity : class
	{

	}

	public class SqlPrimitiveSchemaField<TValue, TEntity> : SchemaField<TEntity>
		where TEntity : class
	{
		public Column Column { get; }

		public bool IsPrimaryKey => PrimaryKeyGenerator != PrimaryKeyGenerator.NotPrimaryKey;

		public PrimaryKeyGenerator PrimaryKeyGenerator { get; }

		public string FieldName { get; }

		public ISchemaFieldReference SchemaFieldReference { get; }

		public FieldType FieldType { get; }

		public string AliasName { get; }

		public EntityFieldJoin Join { get; }

		public System.Type DataType => typeof(TValue);

		public string[] ModelPath { get; }

		public SqlPrimitiveSchemaField(string fieldName, Column column, PrimaryKeyGenerator primaryKeyGenerator,
			EntityFieldJoin join, string[] modelPath,
			string aliasName)
		{
			FieldName = fieldName;
			Column = column;
			PrimaryKeyGenerator = primaryKeyGenerator;
			FieldType = join != null ? FieldType.JoinedField : FieldType.StoredField;
			Join = join;
			ModelPath = modelPath;
			AliasName = aliasName;
			SchemaFieldReference = SchemaFieldReference<TValue>.Create(aliasName);
		}
	}

	public class ProjectedPrimitiveSchemaField<TEntity> : SchemaField<TEntity>
		where TEntity : class
	{
		public Column Column { get; }

		public bool IsPrimaryKey => PrimaryKeyGenerator != PrimaryKeyGenerator.NotPrimaryKey;

		public PrimaryKeyGenerator PrimaryKeyGenerator { get; }

		public string FieldName { get; }

		public ISchemaFieldReference SchemaFieldReference { get; }

		public FieldType FieldType { get; }

		public string AliasName { get; }

		public EntityFieldJoin Join { get; }

		public System.Type DataType { get; }

		public string[] ModelPath { get; }

		public ProjectedPrimitiveSchemaField(string fieldName, Column column, PrimaryKeyGenerator primaryKeyGenerator,
			EntityFieldJoin join, string[] modelPath,
			string aliasName, System.Type dataType, ISchemaFieldReference schemaFieldReference)
		{
			FieldName = fieldName;
			Column = column;
			PrimaryKeyGenerator = primaryKeyGenerator;
			FieldType = join != null ? FieldType.JoinedField : FieldType.StoredField;
			Join = join;
			ModelPath = modelPath;
			AliasName = aliasName;
			DataType = dataType;
			SchemaFieldReference = schemaFieldReference;
		}
	}

	public class EmbeddedObjectNullCheckSchemaField<TValue, TEntity> : SchemaField<TEntity>
		where TEntity : class
	{
		public Column Column { get; }

		public bool IsPrimaryKey => false;

		public PrimaryKeyGenerator PrimaryKeyGenerator => PrimaryKeyGenerator.NotPrimaryKey;

		public string FieldName { get; }

		public ISchemaFieldReference SchemaFieldReference { get; }

		public FieldType FieldType { get; }

		public string AliasName { get; }

		public EntityFieldJoin Join { get; }

		public System.Type DataType => typeof(TValue);

		public string[] ModelPath { get; }

		public EmbeddedObjectNullCheckSchemaField(string fieldName, string columnName,
			EntityFieldJoin join,
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
			ModelPath = modelPath;
			AliasName = aliasName;
			SchemaFieldReference = SchemaFieldReference<bool>.Create(aliasName);
		}
	}

	public class JoinedObjectSchemaField<TValue, TEntity, TPrimaryKey> : SchemaField<TEntity>
		where TEntity : class
		where TValue : class
	{
		public string FieldName { get; }

		public Column Column { get; }

		public bool IsPrimaryKey => false;

		public PrimaryKeyGenerator PrimaryKeyGenerator => PrimaryKeyGenerator.NotPrimaryKey;

		public ISchemaFieldReference SchemaFieldReference { get; }

		public FieldType FieldType { get; }

		public string AliasName { get; }

		public EntityFieldJoin Join { get; }

		public System.Type DataType => typeof(TPrimaryKey);

		public string[] ModelPath { get; }

		public JoinedObjectSchemaField(string fieldName, string columnName,
			EntityFieldJoin join,
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
			ModelPath = modelPath;
			AliasName = aliasName;
			SchemaFieldReference = SchemaFieldReference<TPrimaryKey>.Create(aliasName);
		}
	}
}
