namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// A collection of utility classes for working with fields and field values.
	/// </summary>
	public abstract class FieldOperations
	{
		public IFieldExpressionFactory Expressions { get; }

		public FieldOperations(IFieldExpressionFactory expressions)
		{
			Expressions = expressions;
		}
	}

	public class FieldOperations<TEntity> : FieldOperations
		where TEntity : class
	{
		public new IFieldExpressionFactory<TEntity> Expressions { get; }

		public FieldOperations(IFieldExpressionFactory<TEntity> expressions)
			: base(expressions)
		{
			Expressions = expressions;
		}
	}
}
