namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Represents an entity field in the process of being assembled.
	/// </summary>
	public interface ISchemaFieldAssemblage
	{
		ISchemaFieldBuilder Builder { get; }
		SchemaFieldDefinition FieldDefinition { get; }
		string[] ModelPath { get; }
	}

	public class SqlPrimitiveSchemaFieldAssemblage<TValue, TEntity> : ISchemaFieldAssemblage
		where TEntity : class
	{
		public ISchemaFieldBuilder Builder { get; }

		public SchemaFieldDefinition FieldDefinition { get; }

		public string[] ModelPath { get; }

		public SqlPrimitiveSchemaFieldAssemblage(
			string[] modelPath,
			SqlPrimitiveSchemaFieldBuilder<TValue, TEntity> builder,
			SchemaFieldDefinition<TValue, TEntity> fieldDefinition
			)
		{
			ModelPath = modelPath;
			Builder = builder;
			FieldDefinition = fieldDefinition;
		}
	}

	public class ObjectSchemaFieldAssemblage<TValue, TEntity> : ISchemaFieldAssemblage
		where TEntity : class
	{
		public ISchemaFieldBuilder Builder { get; }

		public SchemaFieldDefinition FieldDefinition { get; }

		public string[] ModelPath { get; }

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
	}
}
