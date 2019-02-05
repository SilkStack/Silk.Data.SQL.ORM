using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Queries;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM
{
	public static class GroupByExtensions
	{
		public static TBuilder GroupBy<TBuilder>(this TBuilder builder, QueryExpression queryExpression)
			where TBuilder : IGroupByQueryBuilder
		{
			builder.GroupBy.GroupBy(queryExpression);
			return builder;
		}

		public static TBuilder GroupBy<TBuilder>(this TBuilder builder, ExpressionResult expressionResult)
			where TBuilder : IGroupByQueryBuilder
		{
			builder.GroupBy.GroupBy(expressionResult);
			return builder;
		}

		private static TBuilder GroupBy<TBuilder, TEntity, TProperty>(this TBuilder builder, Expression<Func<TEntity, TProperty>> expression)
			where TEntity : class
			where TBuilder : IGroupByQueryBuilder<TEntity>
		{
			builder.GroupBy.GroupBy(expression);
			return builder;
		}

		public static IEntitySelectQueryBuilder<TEntity> GroupBy<TEntity, TProperty>(this IEntitySelectQueryBuilder<TEntity> builder, Expression<Func<TEntity, TProperty>> expression)
			where TEntity : class => builder.GroupBy<IEntitySelectQueryBuilder<TEntity>, TEntity, TProperty>(expression);
	}
}
