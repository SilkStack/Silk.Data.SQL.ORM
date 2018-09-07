using Silk.Data.SQL.Expressions;
using System.Linq.Expressions;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class RandomCallConverter : IMethodCallConverter
	{
		public ExpressionResult Convert(MethodInfo methodInfo, MethodCallExpression node,
			ExpressionConverter expressionConverter)
		{
			return new ExpressionResult(QueryExpression.Random());
		}
	}
}
