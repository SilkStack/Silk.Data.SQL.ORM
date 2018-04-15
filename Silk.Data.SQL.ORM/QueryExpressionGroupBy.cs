using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.ORM
{
	public class QueryExpressionGroupBy : GroupBy
	{
		private readonly QueryExpression _queryExpression;

		public QueryExpressionGroupBy(QueryExpression queryExpression)
		{
			_queryExpression = queryExpression;
		}

		public override QueryExpression GetExpression() => _queryExpression;
	}
}
