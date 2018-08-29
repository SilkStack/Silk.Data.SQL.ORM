using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class ExpressionResult
	{
		public QueryExpression QueryExpression { get; }

		public ExpressionResult(QueryExpression queryExpression)
		{
			QueryExpression = queryExpression;
		}
	}
}
