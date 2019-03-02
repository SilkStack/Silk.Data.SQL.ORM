using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM
{
	public static class WhereExtensions
	{
		public static TBuilder AndWhere<TBuilder>(this TBuilder builder, QueryExpression queryExpression)
			where TBuilder : IWhereQueryBuilder
		{
			builder.Where.AndAlso(queryExpression);
			return builder;
		}

		public static TBuilder OrWhere<TBuilder>(this TBuilder builder, QueryExpression queryExpression)
			where TBuilder : IWhereQueryBuilder
		{
			builder.Where.OrElse(queryExpression);
			return builder;
		}

		public static TBuilder AndWhere<TBuilder>(this TBuilder builder, ExpressionResult expressionResult)
			where TBuilder : IWhereQueryBuilder
		{
			builder.Where.AndAlso(expressionResult);
			return builder;
		}

		public static TBuilder OrWhere<TBuilder>(this TBuilder builder, ExpressionResult expressionResult)
			where TBuilder : IWhereQueryBuilder
		{
			builder.Where.OrElse(expressionResult);
			return builder;
		}

		private static TBuilder AndWhere<TBuilder, TEntity>(this TBuilder builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class
			where TBuilder : IWhereQueryBuilder<TEntity>
		{
			builder.Where.AndAlso(expression);
			return builder;
		}

		private static TBuilder OrWhere<TBuilder, TEntity>(this TBuilder builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class
			where TBuilder : IWhereQueryBuilder<TEntity>
		{
			builder.Where.OrElse(expression);
			return builder;
		}

		private static TBuilder AndWhere<TBuilder, TEntity, TValue>(this TBuilder builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class
			where TBuilder : IWhereQueryBuilder<TEntity>
		{
			if (typeof(TValue) == typeof(TEntity))
				builder.Where.AndAlso(schemaField, @operator, value as TEntity);
			else
				builder.Where.AndAlso(schemaField, @operator, value);
			return builder;
		}

		private static TBuilder AndWhere<TBuilder, TEntity>(this TBuilder builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class
			where TBuilder : IWhereQueryBuilder<TEntity>
		{
			builder.Where.AndAlso(schemaField, @operator, valueExpression);
			return builder;
		}

		private static TBuilder AndWhere<TBuilder, TEntity>(this TBuilder builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class
			where TBuilder : IWhereQueryBuilder<TEntity>
		{
			builder.Where.AndAlso(schemaField, @operator, subQuery);
			return builder;
		}

		private static TBuilder OrWhere<TBuilder, TEntity, TValue>(this TBuilder builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class
			where TBuilder : IWhereQueryBuilder<TEntity>
		{
			if (typeof(TValue) == typeof(TEntity))
				builder.Where.OrElse(schemaField, @operator, value as TEntity);
			else
				builder.Where.OrElse(schemaField, @operator, value);
			return builder;
		}

		private static TBuilder OrWhere<TBuilder, TEntity>(this TBuilder builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class
			where TBuilder : IWhereQueryBuilder<TEntity>
		{
			builder.Where.OrElse(schemaField, @operator, valueExpression);
			return builder;
		}

		private static TBuilder OrWhere<TBuilder, TEntity>(this TBuilder builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class
			where TBuilder : IWhereQueryBuilder<TEntity>
		{
			builder.Where.OrElse(schemaField, @operator, subQuery);
			return builder;
		}

		public static IEntitySelectQueryBuilder<TEntity> AndWhere<TEntity>(this IEntitySelectQueryBuilder<TEntity> builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class => builder.AndWhere<IEntitySelectQueryBuilder<TEntity>, TEntity>(expression);
		public static IEntitySelectQueryBuilder<TEntity> OrWhere<TEntity>(this IEntitySelectQueryBuilder<TEntity> builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class => builder.OrWhere<IEntitySelectQueryBuilder<TEntity>, TEntity>(expression);
		public static IEntitySelectQueryBuilder<TEntity> AndWhere<TEntity, TValue>(this IEntitySelectQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class => builder.AndWhere<IEntitySelectQueryBuilder<TEntity>, TEntity, TValue>(schemaField, @operator, value);
		public static IEntitySelectQueryBuilder<TEntity> AndWhere<TEntity>(this IEntitySelectQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class => builder.AndWhere<IEntitySelectQueryBuilder<TEntity>, TEntity>(schemaField, @operator, valueExpression);
		public static IEntitySelectQueryBuilder<TEntity> AndWhere<TEntity>(this IEntitySelectQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class => builder.AndWhere<IEntitySelectQueryBuilder<TEntity>, TEntity>(schemaField, @operator, subQuery);
		public static IEntitySelectQueryBuilder<TEntity> OrWhere<TEntity, TValue>(this IEntitySelectQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class => builder.OrWhere<IEntitySelectQueryBuilder<TEntity>, TEntity, TValue>(schemaField, @operator, value);
		public static IEntitySelectQueryBuilder<TEntity> OrWhere<TEntity>(this IEntitySelectQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class => builder.OrWhere<IEntitySelectQueryBuilder<TEntity>, TEntity>(schemaField, @operator, valueExpression);
		public static IEntitySelectQueryBuilder<TEntity> OrWhere<TEntity>(this IEntitySelectQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class => builder.OrWhere<IEntitySelectQueryBuilder<TEntity>, TEntity>(schemaField, @operator, subQuery);

		public static IEntityUpdateQueryBuilder<TEntity> AndWhere<TEntity>(this IEntityUpdateQueryBuilder<TEntity> builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class => builder.AndWhere<IEntityUpdateQueryBuilder<TEntity>, TEntity>(expression);
		public static IEntityUpdateQueryBuilder<TEntity> OrWhere<TEntity>(this IEntityUpdateQueryBuilder<TEntity> builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class => builder.OrWhere<IEntityUpdateQueryBuilder<TEntity>, TEntity>(expression);
		public static IEntityUpdateQueryBuilder<TEntity> AndWhere<TEntity, TValue>(this IEntityUpdateQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class => builder.AndWhere<IEntityUpdateQueryBuilder<TEntity>, TEntity, TValue>(schemaField, @operator, value);
		public static IEntityUpdateQueryBuilder<TEntity> AndWhere<TEntity>(this IEntityUpdateQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class => builder.AndWhere<IEntityUpdateQueryBuilder<TEntity>, TEntity>(schemaField, @operator, valueExpression);
		public static IEntityUpdateQueryBuilder<TEntity> AndWhere<TEntity>(this IEntityUpdateQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class => builder.AndWhere<IEntityUpdateQueryBuilder<TEntity>, TEntity>(schemaField, @operator, subQuery);
		public static IEntityUpdateQueryBuilder<TEntity> OrWhere<TEntity, TValue>(this IEntityUpdateQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class => builder.OrWhere<IEntityUpdateQueryBuilder<TEntity>, TEntity, TValue>(schemaField, @operator, value);
		public static IEntityUpdateQueryBuilder<TEntity> OrWhere<TEntity>(this IEntityUpdateQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class => builder.OrWhere<IEntityUpdateQueryBuilder<TEntity>, TEntity>(schemaField, @operator, valueExpression);
		public static IEntityUpdateQueryBuilder<TEntity> OrWhere<TEntity>(this IEntityUpdateQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class => builder.OrWhere<IEntityUpdateQueryBuilder<TEntity>, TEntity>(schemaField, @operator, subQuery);

		public static IEntityDeleteQueryBuilder<TEntity> AndWhere<TEntity>(this IEntityDeleteQueryBuilder<TEntity> builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class => builder.AndWhere<IEntityDeleteQueryBuilder<TEntity>, TEntity>(expression);
		public static IEntityDeleteQueryBuilder<TEntity> OrWhere<TEntity>(this IEntityDeleteQueryBuilder<TEntity> builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class => builder.OrWhere<IEntityDeleteQueryBuilder<TEntity>, TEntity>(expression);
		public static IEntityDeleteQueryBuilder<TEntity> AndWhere<TEntity, TValue>(this IEntityDeleteQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class => builder.AndWhere<IEntityDeleteQueryBuilder<TEntity>, TEntity, TValue>(schemaField, @operator, value);
		public static IEntityDeleteQueryBuilder<TEntity> AndWhere<TEntity>(this IEntityDeleteQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class => builder.AndWhere<IEntityDeleteQueryBuilder<TEntity>, TEntity>(schemaField, @operator, valueExpression);
		public static IEntityDeleteQueryBuilder<TEntity> AndWhere<TEntity>(this IEntityDeleteQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class => builder.AndWhere<IEntityDeleteQueryBuilder<TEntity>, TEntity>(schemaField, @operator, subQuery);
		public static IEntityDeleteQueryBuilder<TEntity> OrWhere<TEntity, TValue>(this IEntityDeleteQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class => builder.OrWhere<IEntityDeleteQueryBuilder<TEntity>, TEntity, TValue>(schemaField, @operator, value);
		public static IEntityDeleteQueryBuilder<TEntity> OrWhere<TEntity>(this IEntityDeleteQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class => builder.OrWhere<IEntityDeleteQueryBuilder<TEntity>, TEntity>(schemaField, @operator, valueExpression);
		public static IEntityDeleteQueryBuilder<TEntity> OrWhere<TEntity>(this IEntityDeleteQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class => builder.OrWhere<IEntityDeleteQueryBuilder<TEntity>, TEntity>(schemaField, @operator, subQuery);

		public static DeferableDelete<TEntity> AndWhere<TEntity>(this DeferableDelete<TEntity> builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class => builder.AndWhere<DeferableDelete<TEntity>, TEntity>(expression);
		public static DeferableDelete<TEntity> OrWhere<TEntity>(this DeferableDelete<TEntity> builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class => builder.OrWhere<DeferableDelete<TEntity>, TEntity>(expression);
		public static DeferableDelete<TEntity> AndWhere<TEntity, TValue>(this DeferableDelete<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class => builder.AndWhere<DeferableDelete<TEntity>, TEntity, TValue>(schemaField, @operator, value);
		public static DeferableDelete<TEntity> AndWhere<TEntity>(this DeferableDelete<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class => builder.AndWhere<DeferableDelete<TEntity>, TEntity>(schemaField, @operator, valueExpression);
		public static DeferableDelete<TEntity> AndWhere<TEntity>(this DeferableDelete<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class => builder.AndWhere<DeferableDelete<TEntity>, TEntity>(schemaField, @operator, subQuery);
		public static DeferableDelete<TEntity> OrWhere<TEntity, TValue>(this DeferableDelete<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class => builder.OrWhere<DeferableDelete<TEntity>, TEntity, TValue>(schemaField, @operator, value);
		public static DeferableDelete<TEntity> OrWhere<TEntity>(this DeferableDelete<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class => builder.OrWhere<DeferableDelete<TEntity>, TEntity>(schemaField, @operator, valueExpression);
		public static DeferableDelete<TEntity> OrWhere<TEntity>(this DeferableDelete<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class => builder.OrWhere<DeferableDelete<TEntity>, TEntity>(schemaField, @operator, subQuery);

		public static DeferableUpdate<TEntity> AndWhere<TEntity>(this DeferableUpdate<TEntity> builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class => builder.AndWhere<DeferableUpdate<TEntity>, TEntity>(expression);
		public static DeferableUpdate<TEntity> OrWhere<TEntity>(this DeferableUpdate<TEntity> builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class => builder.OrWhere<DeferableUpdate<TEntity>, TEntity>(expression);
		public static DeferableUpdate<TEntity> AndWhere<TEntity, TValue>(this DeferableUpdate<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class => builder.AndWhere<DeferableUpdate<TEntity>, TEntity, TValue>(schemaField, @operator, value);
		public static DeferableUpdate<TEntity> AndWhere<TEntity>(this DeferableUpdate<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class => builder.AndWhere<DeferableUpdate<TEntity>, TEntity>(schemaField, @operator, valueExpression);
		public static DeferableUpdate<TEntity> AndWhere<TEntity>(this DeferableUpdate<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class => builder.AndWhere<DeferableUpdate<TEntity>, TEntity>(schemaField, @operator, subQuery);
		public static DeferableUpdate<TEntity> OrWhere<TEntity, TValue>(this DeferableUpdate<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class => builder.OrWhere<DeferableUpdate<TEntity>, TEntity, TValue>(schemaField, @operator, value);
		public static DeferableUpdate<TEntity> OrWhere<TEntity>(this DeferableUpdate<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class => builder.OrWhere<DeferableUpdate<TEntity>, TEntity>(schemaField, @operator, valueExpression);
		public static DeferableUpdate<TEntity> OrWhere<TEntity>(this DeferableUpdate<TEntity> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class => builder.OrWhere<DeferableUpdate<TEntity>, TEntity>(schemaField, @operator, subQuery);

		public static SingleDeferableSelect<TEntity, TView> AndWhere<TEntity, TView>(this SingleDeferableSelect<TEntity, TView> builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class => builder.AndWhere<SingleDeferableSelect<TEntity, TView>, TEntity>(expression);
		public static SingleDeferableSelect<TEntity, TView> OrWhere<TEntity, TView>(this SingleDeferableSelect<TEntity, TView> builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class => builder.OrWhere<SingleDeferableSelect<TEntity, TView>, TEntity>(expression);
		public static SingleDeferableSelect<TEntity, TView> AndWhere<TEntity, TView, TValue>(this SingleDeferableSelect<TEntity, TView> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class => builder.AndWhere<SingleDeferableSelect<TEntity, TView>, TEntity, TValue>(schemaField, @operator, value);
		public static SingleDeferableSelect<TEntity, TView> AndWhere<TEntity, TView>(this SingleDeferableSelect<TEntity, TView> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class => builder.AndWhere<SingleDeferableSelect<TEntity, TView>, TEntity>(schemaField, @operator, valueExpression);
		public static SingleDeferableSelect<TEntity, TView> AndWhere<TEntity, TView>(this SingleDeferableSelect<TEntity, TView> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class => builder.AndWhere<SingleDeferableSelect<TEntity, TView>, TEntity>(schemaField, @operator, subQuery);
		public static SingleDeferableSelect<TEntity, TView> OrWhere<TEntity, TView, TValue>(this SingleDeferableSelect<TEntity, TView> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class => builder.OrWhere<SingleDeferableSelect<TEntity, TView>, TEntity, TValue>(schemaField, @operator, value);
		public static SingleDeferableSelect<TEntity, TView> OrWhere<TEntity, TView>(this SingleDeferableSelect<TEntity, TView> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class => builder.OrWhere<SingleDeferableSelect<TEntity, TView>, TEntity>(schemaField, @operator, valueExpression);
		public static SingleDeferableSelect<TEntity, TView> OrWhere<TEntity, TView>(this SingleDeferableSelect<TEntity, TView> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class => builder.OrWhere<SingleDeferableSelect<TEntity, TView>, TEntity>(schemaField, @operator, subQuery);

		public static MultipleDeferableSelect<TEntity, TView> AndWhere<TEntity, TView>(this MultipleDeferableSelect<TEntity, TView> builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class => builder.AndWhere<MultipleDeferableSelect<TEntity, TView>, TEntity>(expression);
		public static MultipleDeferableSelect<TEntity, TView> OrWhere<TEntity, TView>(this MultipleDeferableSelect<TEntity, TView> builder, Expression<Func<TEntity, bool>> expression)
			where TEntity : class => builder.OrWhere<MultipleDeferableSelect<TEntity, TView>, TEntity>(expression);
		public static MultipleDeferableSelect<TEntity, TView> AndWhere<TEntity, TView, TValue>(this MultipleDeferableSelect<TEntity, TView> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class => builder.AndWhere<MultipleDeferableSelect<TEntity, TView>, TEntity, TValue>(schemaField, @operator, value);
		public static MultipleDeferableSelect<TEntity, TView> AndWhere<TEntity, TView>(this MultipleDeferableSelect<TEntity, TView> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class => builder.AndWhere<MultipleDeferableSelect<TEntity, TView>, TEntity>(schemaField, @operator, valueExpression);
		public static MultipleDeferableSelect<TEntity, TView> AndWhere<TEntity, TView>(this MultipleDeferableSelect<TEntity, TView> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class => builder.AndWhere<MultipleDeferableSelect<TEntity, TView>, TEntity>(schemaField, @operator, subQuery);
		public static MultipleDeferableSelect<TEntity, TView> OrWhere<TEntity, TView, TValue>(this MultipleDeferableSelect<TEntity, TView> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, TValue value)
			where TEntity : class => builder.OrWhere<MultipleDeferableSelect<TEntity, TView>, TEntity, TValue>(schemaField, @operator, value);
		public static MultipleDeferableSelect<TEntity, TView> OrWhere<TEntity, TView>(this MultipleDeferableSelect<TEntity, TView> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, Expression<Func<TEntity, bool>> valueExpression)
			where TEntity : class => builder.OrWhere<MultipleDeferableSelect<TEntity, TView>, TEntity>(schemaField, @operator, valueExpression);
		public static MultipleDeferableSelect<TEntity, TView> OrWhere<TEntity, TView>(this MultipleDeferableSelect<TEntity, TView> builder, EntityField<TEntity> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
			where TEntity : class => builder.OrWhere<MultipleDeferableSelect<TEntity, TView>, TEntity>(schemaField, @operator, subQuery);
	}
}
