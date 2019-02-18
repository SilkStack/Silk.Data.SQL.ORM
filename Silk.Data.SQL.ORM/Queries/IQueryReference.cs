using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	/// <summary>
	/// A reference to an object in a query.
	/// ie. an alias, a column, a join, etc.
	/// </summary>
	public interface IQueryReference
	{
		AliasIdentifierExpression AliasIdentifierExpression { get; }
	}
}
