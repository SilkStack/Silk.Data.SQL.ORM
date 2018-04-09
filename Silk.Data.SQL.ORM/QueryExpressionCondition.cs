using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.ORM
{
	public class QueryExpressionCondition : Condition
	{
		private readonly QueryExpression _queryExpression;

		public QueryExpressionCondition(QueryExpression queryExpression)
		{
			_queryExpression = queryExpression;
		}

		public override QueryExpression GetExpression() => _queryExpression;
	}
}
