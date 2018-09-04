using Silk.Data.SQL.Expressions;
using System.Linq.Expressions;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class CountCallConverter : IMethodCallConverter
	{
		public ExpressionResult Convert<TEntity>(MethodInfo methodInfo, MethodCallExpression node,
			ExpressionConverter<TEntity> expressionConverter)
			where TEntity : class
		{
			var expressionResult = expressionConverter.Convert(node.Arguments[0]);
			var countExpression = expressionResult.QueryExpression;
			if (countExpression is TableExpression)
				countExpression = QueryExpression.All();
			return new ExpressionResult(
				QueryExpression.CountFunction(countExpression),
				expressionResult.RequiredJoins
				);
		}
	}
}
