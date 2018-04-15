using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.ORM
{
	public class QueryExpressionOrderBy : OrderBy
	{
		private readonly QueryExpression _queryExpression;

		public QueryExpressionOrderBy(QueryExpression queryExpression)
		{
			_queryExpression = queryExpression;
		}

		public override QueryExpression GetExpression() => _queryExpression;
	}
}
