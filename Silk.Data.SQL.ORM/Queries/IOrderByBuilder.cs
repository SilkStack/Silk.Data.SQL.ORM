using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public interface IOrderByBuilder
	{
		ExpressionResult[] Build();

		void Ascending(QueryExpression queryExpression);
		void Descending(QueryExpression queryExpression);

		void Ascending(ExpressionResult expressionResult);
		void Descending(ExpressionResult expressionResult);
	}

	public interface IEntityOrderByBuilder<T> : IOrderByBuilder
	{
		void Ascending<TProperty>(Expression<Func<T, TProperty>> expression);
		void Descending<TProperty>(Expression<Func<T, TProperty>> expression);
	}
}
