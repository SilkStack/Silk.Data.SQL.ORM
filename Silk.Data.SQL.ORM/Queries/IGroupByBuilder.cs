using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public interface IGroupByBuilder
	{
		void GroupBy(QueryExpression queryExpression);
		void GroupBy(ExpressionResult expressionResult);
		ExpressionResult[] Build();
	}

	public interface IEntityGroupByBuilder<T> : IGroupByBuilder
	{
		void GroupBy<TProperty>(Expression<Func<T, TProperty>> expression);
	}
}
