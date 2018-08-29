using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class ExpressionResult
	{
		public QueryExpression QueryExpression { get; }
		public EntityFieldJoin[] RequiredJoins { get; }

		public ExpressionResult(QueryExpression queryExpression)
		{
			QueryExpression = queryExpression;
		}

		public ExpressionResult(QueryExpression queryExpression, EntityFieldJoin[] requiredJoins)
		{
			QueryExpression = queryExpression;
			RequiredJoins = requiredJoins;
		}
	}
}
