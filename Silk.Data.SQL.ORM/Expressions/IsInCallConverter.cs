using Silk.Data.SQL.Expressions;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class IsInCallConverter : IMethodCallConverter
	{
		public ExpressionResult Convert<TEntity>(MethodInfo methodInfo, MethodCallExpression node,
			ExpressionConverter<TEntity> expressionConverter)
			where TEntity : class
		{
			var searchForExpression = expressionConverter.Convert(node.Arguments[0]);
			var searchInExpression = expressionConverter.Convert(node.Arguments[1]);

			return new ExpressionResult(
				QueryExpression.Compare(searchForExpression.QueryExpression, ComparisonOperator.None,
					QueryExpression.InFunction(searchInExpression.QueryExpression)),
				MethodHelper.ConcatJoins(searchForExpression, searchInExpression).ToArray()
				);
		}
	}
}
