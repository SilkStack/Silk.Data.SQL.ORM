using Silk.Data.SQL.Expressions;
using System.Collections;
using System.Collections.Generic;
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

			if (searchInExpression.QueryExpression is ValueExpression valueExpression &&
				valueExpression.Value is IEnumerable valueEnumerable)
			{
				var valueExpressions = new List<QueryExpression>();
				foreach (var value in valueEnumerable)
				{
					valueExpressions.Add(QueryExpression.Value(value));
				}

				return new ExpressionResult(
					QueryExpression.Compare(searchForExpression.QueryExpression, ComparisonOperator.None,
						QueryExpression.InFunction(valueExpressions.ToArray())),
					MethodHelper.ConcatJoins(searchForExpression, searchInExpression).ToArray()
					);
			}

			return new ExpressionResult(
				QueryExpression.Compare(searchForExpression.QueryExpression, ComparisonOperator.None,
					QueryExpression.InFunction(searchInExpression.QueryExpression)),
				MethodHelper.ConcatJoins(searchForExpression, searchInExpression).ToArray()
				);
		}
	}
}
