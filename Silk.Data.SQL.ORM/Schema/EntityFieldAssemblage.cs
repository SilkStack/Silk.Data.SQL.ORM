namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Represents an entity field in the process of being assembled.
	/// </summary>
	public interface IEntityFieldAssemblage
	{
		IEntityFieldBuilder Builder { get; }
		EntityFieldDefinition FieldDefinition { get; }
		string[] ModelPath { get; }
	}

	public class SqlPrimitiveEntityFieldAssemblage<TValue, TEntity> : IEntityFieldAssemblage
		where TEntity : class
	{
		public IEntityFieldBuilder Builder { get; }

		public EntityFieldDefinition FieldDefinition { get; }

		public string[] ModelPath { get; }

		public SqlPrimitiveEntityFieldAssemblage(
			string[] modelPath,
			SqlPrimitiveEntityFieldBuilder<TValue, TEntity> builder,
			EntityFieldDefinition<TValue, TEntity> fieldDefinition
			)
		{
			ModelPath = modelPath;
			Builder = builder;
			FieldDefinition = fieldDefinition;
		}
	}

	public class ObjectEntityFieldAssemblage<TValue, TEntity> : IEntityFieldAssemblage
		where TEntity : class
	{
		public IEntityFieldBuilder Builder { get; }

		public EntityFieldDefinition FieldDefinition { get; }

		public string[] ModelPath { get; }

		public ObjectEntityFieldAssemblage(
			string[] modelPath,
			ObjectEntityFieldBuilder<TValue, TEntity> builder,
			EntityFieldDefinition<TValue, TEntity> fieldDefinition
			)
		{
			ModelPath = modelPath;
			Builder = builder;
			FieldDefinition = fieldDefinition;
		}
	}
}
