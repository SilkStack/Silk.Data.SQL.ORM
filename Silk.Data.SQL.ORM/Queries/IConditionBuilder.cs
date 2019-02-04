using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public interface IConditionBuilder
	{
		void AndAlso(QueryExpression queryExpression);
		void OrElse(QueryExpression queryExpression);

		void AndAlso(ExpressionResult expressionResult);
		void OrElse(ExpressionResult expressionResult);

		ExpressionResult Build();
	}

	public interface IEntityConditionBuilder<T> : IConditionBuilder
	{
		void AndAlso(Expression<Func<T, bool>> expression);
		void OrElse(Expression<Func<T, bool>> expression);
	}
}
