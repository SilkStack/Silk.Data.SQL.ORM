using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Queries;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM
{
	public static class RangeExtensions
	{
		public static TBuilder Offset<TBuilder>(this TBuilder builder, int offset)
			where TBuilder : IRangeQueryBuilder
		{
			builder.Range.Offset(offset);
			return builder;
		}

		public static TBuilder Limit<TBuilder>(this TBuilder builder, int limit)
			where TBuilder : IRangeQueryBuilder
		{
			builder.Range.Limit(limit);
			return builder;
		}

		public static TBuilder Offset<TBuilder>(this TBuilder builder, QueryExpression queryExpression)
			where TBuilder : IRangeQueryBuilder
		{
			builder.Range.Offset(queryExpression);
			return builder;
		}

		public static TBuilder Limit<TBuilder>(this TBuilder builder, QueryExpression queryExpression)
			where TBuilder : IRangeQueryBuilder
		{
			builder.Range.Limit(queryExpression);
			return builder;
		}

		public static TBuilder Offset<TBuilder>(this TBuilder builder, ExpressionResult expressionResult)
			where TBuilder : IRangeQueryBuilder
		{
			builder.Range.Offset(expressionResult);
			return builder;
		}

		public static TBuilder Limit<TBuilder>(this TBuilder builder, ExpressionResult expressionResult)
			where TBuilder : IRangeQueryBuilder
		{
			builder.Range.Limit(expressionResult);
			return builder;
		}

		private static TBuilder Offset<TBuilder, TEntity>(this TBuilder builder, Expression<Func<TEntity, int>> expression)
			where TEntity : class
			where TBuilder : IRangeQueryBuilder<TEntity>
		{
			builder.Range.Offset(expression);
			return builder;
		}

		private static TBuilder Limit<TBuilder, TEntity>(this TBuilder builder, Expression<Func<TEntity, int>> expression)
			where TEntity : class
			where TBuilder : IRangeQueryBuilder<TEntity>
		{
			builder.Range.Limit(expression);
			return builder;
		}

		public static IEntitySelectQueryBuilder<TEntity> Offset<TEntity, TProperty>(this IEntitySelectQueryBuilder<TEntity> builder, Expression<Func<TEntity, int>> expression)
			where TEntity : class => builder.Offset<IEntitySelectQueryBuilder<TEntity>, TEntity>(expression);

		public static IEntitySelectQueryBuilder<TEntity> Limit<TEntity, TProperty>(this IEntitySelectQueryBuilder<TEntity> builder, Expression<Func<TEntity, int>> expression)
			where TEntity : class => builder.Limit<IEntitySelectQueryBuilder<TEntity>, TEntity>(expression);
	}
}
