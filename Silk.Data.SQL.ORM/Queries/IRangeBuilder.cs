using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public interface IRangeBuilder
	{
		void Offset(int offset);
		void Limit(int limit);

		void Offset(QueryExpression queryExpression);
		void Limit(QueryExpression queryExpression);

		void Offset(ExpressionResult expressionResult);
		void Limit(ExpressionResult expressionResult);

		ExpressionResult BuildOffset();
		ExpressionResult BuildLimit();
	}

	public interface IEntityRangeBuilder<T> : IRangeBuilder
	{
		void Offset(Expression<Func<T, int>> expression);
		void Limit(Expression<Func<T, int>> expression);
	}
}
