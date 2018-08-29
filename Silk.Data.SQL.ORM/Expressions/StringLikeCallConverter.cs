using Silk.Data.SQL.Expressions;
using System.Linq.Expressions;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class StringLikeCallConverter : IMethodCallConverter
	{
		public ExpressionResult Convert<TEntity>(MethodInfo methodInfo, MethodCallExpression node,
			ExpressionConverter<TEntity> expressionConverter)
			where TEntity : class
		{
			var searchField = expressionConverter.Convert(node.Arguments[0]);
			var searchText = expressionConverter.Convert(node.Arguments[1]);

			return new ExpressionResult(
				QueryExpression.Compare(searchField.QueryExpression, ComparisonOperator.Like, searchText.QueryExpression)
				);
		}
	}
}
