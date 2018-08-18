using System;
using System.Collections.Generic;
using System.Text;
using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public class ConditionBuilder : IExpressionBuilder
	{
		private QueryExpression _conditionExpression;

		public void AddAnd(Condition condition)
		{
			if (_conditionExpression == null)
			{
				_conditionExpression = condition.GetExpression();
				return;
			}
			_conditionExpression = QueryExpression.CombineConditions(
				_conditionExpression, ConditionType.AndAlso, condition.GetExpression()
				);
		}

		public void AddOr(Condition condition)
		{
			if (_conditionExpression == null)
			{
				_conditionExpression = condition.GetExpression();
				return;
			}
			_conditionExpression = QueryExpression.CombineConditions(
				_conditionExpression, ConditionType.OrElse, condition.GetExpression()
				);
		}

		public QueryExpression BuildExpression()
		{
			return _conditionExpression;
		}
	}
}
