using Silk.Data.Modelling;
using System.Linq;

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
		EntityJoinBuilder Join { get; }
	}

	public interface ISchemaFieldAssemblage<TEntity> : ISchemaFieldAssemblage
		where TEntity : class
	{
		ISchemaFieldBuilder<TEntity> Builder { get; }
		(ISchemaField<T> schemaField, FieldOperations<T> operations) CreateJoinedSchemaFieldAndOperationsPair<T>(
			string fieldName, string columnName, IFieldReference entityFieldReference,
			string[] modelPath, EntityFieldJoin join, IEntitySchemaAssemblage<T> entitySchemaAssemblage
			)
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

		public EntityJoinBuilder Join { get; }

		public SqlPrimitiveSchemaFieldAssemblage(
			string[] modelPath,
			SqlPrimitiveSchemaFieldBuilder<TValue, TEntity> builder,
			SchemaFieldDefinition<TValue, TEntity> fieldDefinition,
			PrimaryKeyGenerator primaryKeyGenerator,
			IEntitySchemaAssemblage<TEntity> entitySchemaAssemblage,
			EntityJoinBuilder join
			)
		{
			ModelPath = modelPath;
			Builder = builder;
			FieldDefinition = fieldDefinition;
			Column = new Column(
				fieldDefinition.ColumnName ?? string.Join("_", modelPath),
				fieldDefinition.SqlDataType,
				fieldDefinition.IsNullable,
				join?.TableAlias ?? entitySchemaAssemblage.TableName
				);
			PrimaryKeyGenerator = primaryKeyGenerator;
			Join = join;
		}

		public (ISchemaField<T> schemaField, FieldOperations<T> operations) CreateJoinedSchemaFieldAndOperationsPair<T>(
			string fieldName, string columnName, IFieldReference entityFieldReference,
			string[] modelPath, EntityFieldJoin join, IEntitySchemaAssemblage<T> entitySchemaAssemblage
			) where T : class
		{
			var typeModel = TypeModel.GetModelOf<T>();
			var pkReference = typeModel.GetFieldReference(new PathOnlySourceField(
				modelPath.Concat(ModelPath).ToArray()
				));
			var field = new JoinedObjectSchemaField<TEntity, T, TValue>(fieldName, columnName, entityFieldReference, join, entitySchemaAssemblage, modelPath);
			var operations = new FieldOperations<T>(
				new JoinedObjectExpressionFactory<TEntity, T, TValue>(field, pkReference)
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

		public EntityJoinBuilder Join { get; }

		public ObjectSchemaFieldAssemblage(
			string[] modelPath,
			ObjectEntityFieldBuilder<TValue, TEntity> builder,
			SchemaFieldDefinition<TValue, TEntity> fieldDefinition,
			EntityJoinBuilder join
			)
		{
			ModelPath = modelPath;
			Builder = builder;
			FieldDefinition = fieldDefinition;
			Join = join;
		}

		public (ISchemaField<T> schemaField, FieldOperations<T> operations) CreateJoinedSchemaFieldAndOperationsPair<T>(
			string fieldName, string columnName, IFieldReference entityFieldReference,
			string[] modelPath, EntityFieldJoin join, IEntitySchemaAssemblage<T> entitySchemaAssemblage
			) where T : class
		{
			var typeModel = TypeModel.GetModelOf<T>();
			var pkReference = typeModel.GetFieldReference(new PathOnlySourceField(
				modelPath.Concat(ModelPath).ToArray()
				));
			var field = new JoinedObjectSchemaField<TEntity, T, TValue>(fieldName, columnName, entityFieldReference, join, entitySchemaAssemblage, modelPath);
			var operations = new FieldOperations<T>(
				new JoinedObjectExpressionFactory<TEntity, T, TValue>(field, pkReference)
				);
			return (field, operations);
		}
	}
}
