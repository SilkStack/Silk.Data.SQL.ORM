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

	public class EntityFieldAssemblage<TValue, TEntity> : IEntityFieldAssemblage
		where TEntity : class
	{
		public IEntityFieldBuilder Builder { get; }

		public EntityFieldDefinition FieldDefinition { get; }

		public string[] ModelPath { get; }

		public EntityFieldAssemblage(
			string[] modelPath,
			EntityFieldBuilder<TValue, TEntity> builder,
			EntityFieldDefinition<TValue, TEntity> fieldDefinition
			)
		{
			ModelPath = modelPath;
			Builder = builder;
			FieldDefinition = fieldDefinition;
		}
	}
}
