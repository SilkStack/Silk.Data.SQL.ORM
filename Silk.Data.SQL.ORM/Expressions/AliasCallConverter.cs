using Silk.Data.SQL.Expressions;
using System.Linq.Expressions;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class AliasCallConverter : IMethodCallConverter
	{
		public ExpressionResult Convert(MethodInfo methodInfo, MethodCallExpression node,
			ExpressionConverter expressionConverter)
		{
			var expression = expressionConverter.Convert(node.Arguments[0]);
			var alias = expressionConverter.Convert(node.Arguments[1]).QueryExpression as ValueExpression;

			return new ExpressionResult(
				QueryExpression.Alias(expression.QueryExpression, (string)alias.Value),
				expression.RequiredJoins
				);
		}
	}
}
