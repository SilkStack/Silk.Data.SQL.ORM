using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM
{
	public static class HavingExtensions
	{
		public static TBuilder AndHaving<TBuilder>(this TBuilder builder, QueryExpression queryExpression)
			where TBuilder : IHavingQueryBuilder
		{
			builder.Having.AndAlso(queryExpression);
			return builder;
		}

		public static TBuilder OrHaving<TBuilder>(this TBuilder builder, QueryExpression queryExpression)
			where TBuilder : IHavingQueryBuilder
		{
			builder.Having.OrElse(queryExpression);
			return builder;
		}

		public static TBuilder AndHaving<TBuilder>(this TBuilder builder, ExpressionResult expressionResult)
			where TBuilder : IHavingQueryBuilder
		{
			builder.Having.AndAlso(expressionResult);
			return builder;
		}

		public static TBuilder OrHaving<TBuilder>(this TBuilder builder, ExpressionResult expressionResult)
			where TBuilder : IHavingQueryBuilder
		{
			builder.Having.OrElse(expressionResult);
			return builder;
		}

		private static TBuilder AndHaving<TBuilder, TEntity>(this TBuilder builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class
			where TBuilder : IHavingQueryBuilder<TEntity>
		{
			builder.Having.AndAlso(expression);
			return builder;
		}

		private static TBuilder OrHaving<TBuilder, TEntity>(this TBuilder builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class
			where TBuilder : IHavingQueryBuilder<TEntity>
		{
			builder.Having.OrElse(expression);
			return builder;
		}

		private static TBuilder AndHaving<TBuilder, TEntity, TValue>(this TBuilder builder, SchemaField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class
			where TBuilder : IHavingQueryBuilder<TEntity>
		{
			if (typeof(TValue) == typeof(TEntity))
				builder.Having.AndAlso(schemaField, @operator, value as TEntity);
			else
				builder.Having.AndAlso(schemaField, @operator, value);
			return builder;
		}

		private static TBuilder AndHaving<TBuilder, TEntity>(this TBuilder builder, SchemaField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class
			where TBuilder : IHavingQueryBuilder<TEntity>
		{
			builder.Having.AndAlso(schemaField, @operator, valueExpression);
			return builder;
		}

		private static TBuilder AndHaving<TBuilder, TEntity>(this TBuilder builder, SchemaField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class
			where TBuilder : IHavingQueryBuilder<TEntity>
		{
			builder.Having.AndAlso(schemaField, @operator, subQuery);
			return builder;
		}

		private static TBuilder OrHaving<TBuilder, TEntity, TValue>(this TBuilder builder, SchemaField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class
			where TBuilder : IHavingQueryBuilder<TEntity>
		{
			if (typeof(TValue) == typeof(TEntity))
				builder.Having.OrElse(schemaField, @operator, value as TEntity);
			else
				builder.Having.OrElse(schemaField, @operator, value);
			return builder;
		}

		private static TBuilder OrHaving<TBuilder, TEntity>(this TBuilder builder, SchemaField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class
			where TBuilder : IHavingQueryBuilder<TEntity>
		{
			builder.Having.OrElse(schemaField, @operator, valueExpression);
			return builder;
		}

		private static TBuilder OrHaving<TBuilder, TEntity>(this TBuilder builder, SchemaField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class
			where TBuilder : IHavingQueryBuilder<TEntity>
		{
			builder.Having.OrElse(schemaField, @operator, subQuery);
			return builder;
		}

		public static IEntitySelectQueryBuilder<TEntity> AndHaving<TEntity>(this IEntitySelectQueryBuilder<TEntity> builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class => builder.AndHaving<IEntitySelectQueryBuilder<TEntity>, TEntity>(expression);
		public static IEntitySelectQueryBuilder<TEntity> OrHaving<TEntity>(this IEntitySelectQueryBuilder<TEntity> builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class => builder.OrHaving<IEntitySelectQueryBuilder<TEntity>, TEntity>(expression);
		public static IEntitySelectQueryBuilder<TEntity> AndHaving<TEntity, TValue>(this IEntitySelectQueryBuilder<TEntity> builder, SchemaField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class => builder.AndHaving<IEntitySelectQueryBuilder<TEntity>, TEntity, TValue>(schemaField, @operator, value);
		public static IEntitySelectQueryBuilder<TEntity> AndHaving<TEntity>(this IEntitySelectQueryBuilder<TEntity> builder, SchemaField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class => builder.AndHaving<IEntitySelectQueryBuilder<TEntity>, TEntity>(schemaField, @operator, valueExpression);
		public static IEntitySelectQueryBuilder<TEntity> AndHaving<TEntity>(this IEntitySelectQueryBuilder<TEntity> builder, SchemaField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class => builder.AndHaving<IEntitySelectQueryBuilder<TEntity>, TEntity>(schemaField, @operator, subQuery);
		public static IEntitySelectQueryBuilder<TEntity> OrHaving<TEntity, TValue>(this IEntitySelectQueryBuilder<TEntity> builder, SchemaField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class => builder.OrHaving<IEntitySelectQueryBuilder<TEntity>, TEntity, TValue>(schemaField, @operator, value);
		public static IEntitySelectQueryBuilder<TEntity> OrHaving<TEntity>(this IEntitySelectQueryBuilder<TEntity> builder, SchemaField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class => builder.OrHaving<IEntitySelectQueryBuilder<TEntity>, TEntity>(schemaField, @operator, valueExpression);
		public static IEntitySelectQueryBuilder<TEntity> OrHaving<TEntity>(this IEntitySelectQueryBuilder<TEntity> builder, SchemaField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class => builder.OrHaving<IEntitySelectQueryBuilder<TEntity>, TEntity>(schemaField, @operator, subQuery);
	}
}
