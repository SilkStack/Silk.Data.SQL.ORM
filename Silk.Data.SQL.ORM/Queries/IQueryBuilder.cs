using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	/// <summary>
	/// A general, non-entity specific query builder.
	/// </summary>
	public interface IQueryBuilder
	{
		QueryExpression BuildQuery();
	}

	public interface IEntityQueryBuilder : IQueryBuilder
	{
		QueryExpression BuildQuery(string tableName);
	}

	public interface IEntityQueryBuilder<T> : IEntityQueryBuilder
		where T : class
	{

	}
}
