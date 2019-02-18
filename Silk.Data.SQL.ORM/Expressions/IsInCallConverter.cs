using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class IsInCallConverter : IMethodCallConverter
	{
		public ExpressionResult Convert(MethodInfo methodInfo, MethodCallExpression node,
			ExpressionConverter expressionConverter)
		{
			var searchForExpression = expressionConverter.Convert(node.Arguments[0]);
			var searchInExpression = expressionConverter.Convert(node.Arguments[1]);

			if (searchInExpression.QueryExpression is ValueExpression valueExpression &&
				valueExpression.Value is IEnumerable valueEnumerable)
			{
				var valueExpressions = new List<QueryExpression>();
				foreach (var value in valueEnumerable)
				{
					valueExpressions.Add(ORMQueryExpressions.Value(value));
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
