using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM
{
	public abstract class ConditionBuilderBase
	{
		private QueryExpression _conditionExpression;

		protected void AddAnd(Condition condition)
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

		protected void AddOr(Condition condition)
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

		public Condition Build()
		{
			return new QueryExpressionCondition(_conditionExpression);
		}
	}

	public class ConditionBuilder : ConditionBuilderBase
	{
		public static ConditionBuilder Create(Condition condition)
		{
			return new ConditionBuilder()
				.And(condition);
		}

		public ConditionBuilder And(Condition condition)
		{
			AddAnd(condition);
			return this;
		}

		public ConditionBuilder Or(Condition condition)
		{
			AddOr(condition);
			return this;
		}
	}

	public class ConditionBuilder<T> : ConditionBuilderBase
	{
		private readonly EntityModel<T> _entityModel;

		public ConditionBuilder(EntityModel<T> entityModel)
		{
			_entityModel = entityModel;
		}

		public ConditionBuilder<T> And(Condition condition)
		{
			AddAnd(condition);
			return this;
		}

		public ConditionBuilder<T> Or(Condition condition)
		{
			AddOr(condition);
			return this;
		}

		public ConditionBuilder<T> And(Expression<Func<T, bool>> condition)
		{
			return And(new ExpressionCondition<T>(condition, _entityModel));
		}

		public ConditionBuilder<T> Or(Expression<Func<T, bool>> condition)
		{
			return Or(new ExpressionCondition<T>(condition, _entityModel));
		}
	}
}
