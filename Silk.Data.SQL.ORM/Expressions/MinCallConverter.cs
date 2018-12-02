using Silk.Data.SQL.Expressions;
using System.Linq.Expressions;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class MinCallConverter : IMethodCallConverter
	{
		public ExpressionResult Convert(MethodInfo methodInfo, MethodCallExpression node,
			ExpressionConverter expressionConverter)
		{
			var expressionResult = expressionConverter.Convert(node.Arguments[0]);
			var minExpression = expressionResult.QueryExpression;
			if (minExpression is TableExpression)
				minExpression = QueryExpression.All();
			return new ExpressionResult(
				QueryExpression.Min(minExpression),
				expressionResult.RequiredJoins
				);
		}
	}
}
