﻿using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;
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
		where T : class
	{
		void AndAlso(Expression<Func<T, bool>> expression);
		void OrElse(Expression<Func<T, bool>> expression);

		void AndAlso(EntityField<T> schemaField, ComparisonOperator @operator, T entity);
		void AndAlso<TValue>(EntityField<T> schemaField, ComparisonOperator @operator, TValue value);
		void AndAlso(EntityField<T> schemaField, ComparisonOperator @operator, Expression<Func<T, bool>> valueExpression);
		void AndAlso(EntityField<T> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery);
		void OrElse(EntityField<T> schemaField, ComparisonOperator @operator, T entity);
		void OrElse<TValue>(EntityField<T> schemaField, ComparisonOperator @operator, TValue value);
		void OrElse(EntityField<T> schemaField, ComparisonOperator @operator, Expression<Func<T, bool>> valueExpression);
		void OrElse(EntityField<T> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery);
	}
}
