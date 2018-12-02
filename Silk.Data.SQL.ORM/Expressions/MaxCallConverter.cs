using Silk.Data.SQL.Expressions;
using System.Linq.Expressions;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class MaxCallConverter : IMethodCallConverter
	{
		public ExpressionResult Convert(MethodInfo methodInfo, MethodCallExpression node,
			ExpressionConverter expressionConverter)
		{
			var expressionResult = expressionConverter.Convert(node.Arguments[0]);
			var maxExpression = expressionResult.QueryExpression;
			if (maxExpression is TableExpression)
				maxExpression = QueryExpression.All();
			return new ExpressionResult(
				QueryExpression.Max(maxExpression),
				expressionResult.RequiredJoins
				);
		}
	}
}
