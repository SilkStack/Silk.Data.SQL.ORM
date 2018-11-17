using Silk.Data.Modelling;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Represents an entity field in the process of being assembled.
	/// </summary>
	public interface ISchemaFieldAssemblage
	{
		SchemaFieldDefinition FieldDefinition { get; }
		string[] ModelPath { get; }
		Column Column { get; }
		PrimaryKeyGenerator PrimaryKeyGenerator { get; }
	}

	public interface ISchemaFieldAssemblage<TEntity> : ISchemaFieldAssemblage
		where TEntity : class
	{
		ISchemaFieldBuilder<TEntity> Builder { get; }
		(ISchemaField<T> schemaField, FieldOperations<T> operations) CreateJoinedSchemaFieldAndOperationsPair<T>(string fieldName, string columnName, IFieldReference entityFieldReference)
			where T : class;
	}

	public class SqlPrimitiveSchemaFieldAssemblage<TValue, TEntity> : ISchemaFieldAssemblage<TEntity>
		where TEntity : class
	{
		public ISchemaFieldBuilder<TEntity> Builder { get; }

		public SchemaFieldDefinition FieldDefinition { get; }

		public string[] ModelPath { get; }

		public Column Column { get; }

		public PrimaryKeyGenerator PrimaryKeyGenerator { get; }

		public SqlPrimitiveSchemaFieldAssemblage(
			string[] modelPath,
			SqlPrimitiveSchemaFieldBuilder<TValue, TEntity> builder,
			SchemaFieldDefinition<TValue, TEntity> fieldDefinition,
			PrimaryKeyGenerator primaryKeyGenerator
			)
		{
			ModelPath = modelPath;
			Builder = builder;
			FieldDefinition = fieldDefinition;
			Column = new Column(
				fieldDefinition.ColumnName ?? string.Join("_", modelPath),
				fieldDefinition.SqlDataType,
				fieldDefinition.IsNullable
				);
			PrimaryKeyGenerator = primaryKeyGenerator;
		}

		public (ISchemaField<T> schemaField, FieldOperations<T> operations) CreateJoinedSchemaFieldAndOperationsPair<T>(string fieldName, string columnName, IFieldReference entityFieldReference) where T : class
		{
			var field = new JoinedObjectSchemaField<T, TEntity, TValue>(fieldName, columnName, entityFieldReference);
			var operations = new FieldOperations<T>(
				new JoinedObjectExpressionFactory<T, TEntity, TValue>(field)
				);
			return (field, operations);
		}
	}

	public class ObjectSchemaFieldAssemblage<TValue, TEntity> : ISchemaFieldAssemblage<TEntity>
		where TEntity : class
		where TValue : class
	{
		public ISchemaFieldBuilder<TEntity> Builder { get; }

		public SchemaFieldDefinition FieldDefinition { get; }

		public string[] ModelPath { get; }

		public Column Column => throw new System.NotImplementedException();

		public PrimaryKeyGenerator PrimaryKeyGenerator => PrimaryKeyGenerator.NotPrimaryKey;

		public ObjectSchemaFieldAssemblage(
			string[] modelPath,
			ObjectEntityFieldBuilder<TValue, TEntity> builder,
			SchemaFieldDefinition<TValue, TEntity> fieldDefinition
			)
		{
			ModelPath = modelPath;
			Builder = builder;
			FieldDefinition = fieldDefinition;
		}

		public (ISchemaField<T> schemaField, FieldOperations<T> operations) CreateJoinedSchemaFieldAndOperationsPair<T>(string fieldName, string columnName, IFieldReference entityFieldReference) where T : class
		{
			var field = new JoinedObjectSchemaField<T, TEntity, TValue>(fieldName, columnName, entityFieldReference);
			var operations = new FieldOperations<T>(
				new JoinedObjectExpressionFactory<T, TEntity, TValue>(field)
				);
			return (field, operations);
		}
	}
}
