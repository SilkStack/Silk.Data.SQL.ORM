using Silk.Data.SQL.Expressions;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class StringLikeCallConverter : IMethodCallConverter
	{
		public ExpressionResult Convert(MethodInfo methodInfo, MethodCallExpression node,
			ExpressionConverter expressionConverter)
		{
			var searchField = expressionConverter.Convert(node.Arguments[0]);
			var searchText = expressionConverter.Convert(node.Arguments[1]);

			return new ExpressionResult(
				QueryExpression.Compare(searchField.QueryExpression, ComparisonOperator.Like, searchText.QueryExpression),
				MethodHelper.ConcatJoins(searchField, searchText).ToArray()
				);
		}
	}
}
