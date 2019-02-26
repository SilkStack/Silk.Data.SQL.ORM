using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Queries;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM
{
	public static class OrderByExtensions
	{
		public static TBuilder OrderBy<TBuilder>(this TBuilder builder, QueryExpression queryExpression)
			where TBuilder : IOrderByQueryBuilder
		{
			builder.OrderBy.Ascending(queryExpression);
			return builder;
		}

		public static TBuilder OrderByDescending<TBuilder>(this TBuilder builder, QueryExpression queryExpression)
			where TBuilder : IOrderByQueryBuilder
		{
			builder.OrderBy.Descending(queryExpression);
			return builder;
		}

		public static TBuilder OrderBy<TBuilder>(this TBuilder builder, ExpressionResult expressionResult)
			where TBuilder : IOrderByQueryBuilder
		{
			builder.OrderBy.Ascending(expressionResult);
			return builder;
		}

		public static TBuilder OrderByDescending<TBuilder>(this TBuilder builder, ExpressionResult expressionResult)
			where TBuilder : IOrderByQueryBuilder
		{
			builder.OrderBy.Descending(expressionResult);
			return builder;
		}

		private static TBuilder OrderBy<TBuilder, TEntity, TProperty>(this TBuilder builder, Expression<Func<TEntity, TProperty>> expression)
			where TEntity : class
			where TBuilder : IOrderByQueryBuilder<TEntity>
		{
			builder.OrderBy.Ascending(expression);
			return builder;
		}

		private static TBuilder OrderByDescending<TBuilder, TEntity, TProperty>(this TBuilder builder, Expression<Func<TEntity, TProperty>> expression)
			where TEntity : class
			where TBuilder : IOrderByQueryBuilder<TEntity>
		{
			builder.OrderBy.Descending(expression);
			return builder;
		}

		public static IEntitySelectQueryBuilder<TEntity> OrderBy<TEntity, TProperty>(this IEntitySelectQueryBuilder<TEntity> builder, Expression<Func<TEntity, TProperty>> expression)
			where TEntity : class => builder.OrderBy<IEntitySelectQueryBuilder<TEntity>, TEntity, TProperty>(expression);

		public static IEntitySelectQueryBuilder<TEntity> OrderByDescending<TEntity, TProperty>(this IEntitySelectQueryBuilder<TEntity> builder, Expression<Func<TEntity, TProperty>> expression)
			where TEntity : class => builder.OrderByDescending<IEntitySelectQueryBuilder<TEntity>, TEntity, TProperty>(expression);
	}
}
