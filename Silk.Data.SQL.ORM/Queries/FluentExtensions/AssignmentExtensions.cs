using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM
{
	public static class AssignmentExtensions
	{
		public static TBuilder Set<TBuilder>(this TBuilder builder, ColumnExpression columnExpression, QueryExpression valueExpression)
			where TBuilder : IFieldAssignmentQueryBuilder
		{
			builder.Assignments.Set(columnExpression, valueExpression);
			return builder;
		}

		public static TBuilder Set<TBuilder>(this TBuilder builder, ColumnExpression columnExpression, object value)
			where TBuilder : IFieldAssignmentQueryBuilder
		{
			builder.Assignments.Set(columnExpression, value);
			return builder;
		}

		private static TBuilder Set<TBuilder, TEntity, TValue>(this TBuilder builder, EntityField<TEntity> schemaField, TValue value)
			where TEntity : class
			where TBuilder : IFieldAssignmentQueryBuilder<TEntity>
		{
			if (typeof(TValue) == typeof(TEntity))
				builder.Assignments.Set(schemaField, value as TEntity);
			else
				builder.Assignments.Set(schemaField, value);
			return builder;
		}

		private static TBuilder Set<TBuilder, TEntity, TValue>(this TBuilder builder, EntityField<TEntity> schemaField, Expression<Func<TEntity, TValue>> valueExpression)
			where TEntity : class
			where TBuilder : IFieldAssignmentQueryBuilder<TEntity>
		{
			builder.Assignments.Set(schemaField, valueExpression);
			return builder;
		}

		private static TBuilder Set<TBuilder, TEntity>(this TBuilder builder, EntityField<TEntity> schemaField, IQueryBuilder subQuery)
			where TEntity : class
			where TBuilder : IFieldAssignmentQueryBuilder<TEntity>
		{
			builder.Assignments.Set(schemaField, subQuery);
			return builder;
		}

		private static TBuilder Set<TBuilder, TEntity, TProperty>(this TBuilder builder, Expression<Func<TEntity, TProperty>> fieldSelector, TProperty value)
			where TEntity : class
			where TBuilder : IFieldAssignmentQueryBuilder<TEntity>
		{
			builder.Assignments.Set(fieldSelector, value);
			return builder;
		}

		private static TBuilder Set<TBuilder, TEntity, TProperty>(this TBuilder builder, Expression<Func<TEntity, TProperty>> fieldSelector, Expression<Func<TEntity, TProperty>> valueExpression)
			where TEntity : class
			where TBuilder : IFieldAssignmentQueryBuilder<TEntity>
		{
			builder.Assignments.Set(fieldSelector, valueExpression);
			return builder;
		}

		private static TBuilder Set<TBuilder, TEntity, TProperty>(this TBuilder builder, Expression<Func<TEntity, TProperty>> fieldSelector, Expression valueExpression)
			where TEntity : class
			where TBuilder : IFieldAssignmentQueryBuilder<TEntity>
		{
			builder.Assignments.Set(fieldSelector, valueExpression);
			return builder;
		}

		private static TBuilder Set<TBuilder, TEntity, TProperty>(this TBuilder builder, Expression<Func<TEntity, TProperty>> fieldSelector, IQueryBuilder subQuery)
			where TEntity : class
			where TBuilder : IFieldAssignmentQueryBuilder<TEntity>
		{
			builder.Assignments.Set(fieldSelector, subQuery);
			return builder;
		}

		private static TBuilder Set<TBuilder, TEntity, TProperty>(this TBuilder builder, Expression<Func<TEntity, TProperty>> fieldSelector, QueryExpression valueExpression)
			where TEntity : class
			where TBuilder : IFieldAssignmentQueryBuilder<TEntity>
		{
			builder.Assignments.Set(fieldSelector, valueExpression);
			return builder;
		}

		public static IEntityInsertQueryBuilder<TEntity> Set<TEntity, TValue>(this IEntityInsertQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, TValue value)
			where TEntity : class => builder.Set<IEntityInsertQueryBuilder<TEntity>, TEntity, TValue>(schemaField, value);

		public static IEntityInsertQueryBuilder<TEntity> Set<TEntity, TValue>(this IEntityInsertQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, Expression<Func<TEntity, TValue>> valueExpression)
			where TEntity : class => builder.Set<IEntityInsertQueryBuilder<TEntity>, TEntity, TValue>(schemaField, valueExpression);

		public static IEntityInsertQueryBuilder<TEntity> Set<TEntity>(this IEntityInsertQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, IQueryBuilder subQuery)
			where TEntity : class => builder.Set<IEntityInsertQueryBuilder<TEntity>, TEntity>(schemaField, subQuery);

		public static IEntityInsertQueryBuilder<TEntity> Set<TEntity, TProperty>(this IEntityInsertQueryBuilder<TEntity> builder, Expression<Func<TEntity, TProperty>> fieldSelector, TProperty value)
			where TEntity : class => builder.Set<IEntityInsertQueryBuilder<TEntity>, TEntity, TProperty>(fieldSelector, value);

		public static IEntityInsertQueryBuilder<TEntity> Set<TEntity, TProperty>(this IEntityInsertQueryBuilder<TEntity> builder, Expression<Func<TEntity, TProperty>> fieldSelector, Expression<Func<TEntity, TProperty>> valueExpression)
			where TEntity : class => builder.Set<IEntityInsertQueryBuilder<TEntity>, TEntity, TProperty>(fieldSelector, valueExpression);

		public static IEntityInsertQueryBuilder<TEntity> Set<TEntity, TProperty>(this IEntityInsertQueryBuilder<TEntity> builder, Expression<Func<TEntity, TProperty>> fieldSelector, Expression valueExpression)
			where TEntity : class => builder.Set<IEntityInsertQueryBuilder<TEntity>, TEntity, TProperty>(fieldSelector, valueExpression);

		public static IEntityInsertQueryBuilder<TEntity> Set<TEntity, TProperty>(this IEntityInsertQueryBuilder<TEntity> builder, Expression<Func<TEntity, TProperty>> fieldSelector, IQueryBuilder subQuery)
			where TEntity : class => builder.Set<IEntityInsertQueryBuilder<TEntity>, TEntity, TProperty>(fieldSelector, subQuery);

		public static IEntityInsertQueryBuilder<TEntity> Set<TEntity, TProperty>(this IEntityInsertQueryBuilder<TEntity> builder, Expression<Func<TEntity, TProperty>> fieldSelector, QueryExpression valueExpression)
			where TEntity : class => builder.Set<IEntityInsertQueryBuilder<TEntity>, TEntity, TProperty>(fieldSelector, valueExpression);

		public static IEntityUpdateQueryBuilder<TEntity> Set<TEntity, TValue>(this IEntityUpdateQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, TValue value)
			where TEntity : class => builder.Set<IEntityUpdateQueryBuilder<TEntity>, TEntity, TValue>(schemaField, value);

		public static IEntityUpdateQueryBuilder<TEntity> Set<TEntity, TValue>(this IEntityUpdateQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, Expression<Func<TEntity, TValue>> valueExpression)
			where TEntity : class => builder.Set<IEntityUpdateQueryBuilder<TEntity>, TEntity, TValue>(schemaField, valueExpression);

		public static IEntityUpdateQueryBuilder<TEntity> Set<TEntity>(this IEntityUpdateQueryBuilder<TEntity> builder, EntityField<TEntity> schemaField, IQueryBuilder subQuery)
			where TEntity : class => builder.Set<IEntityUpdateQueryBuilder<TEntity>, TEntity>(schemaField, subQuery);

		public static IEntityUpdateQueryBuilder<TEntity> Set<TEntity, TProperty>(this IEntityUpdateQueryBuilder<TEntity> builder, Expression<Func<TEntity, TProperty>> fieldSelector, TProperty value)
			where TEntity : class => builder.Set<IEntityUpdateQueryBuilder<TEntity>, TEntity, TProperty>(fieldSelector, value);

		public static IEntityUpdateQueryBuilder<TEntity> Set<TEntity, TProperty>(this IEntityUpdateQueryBuilder<TEntity> builder, Expression<Func<TEntity, TProperty>> fieldSelector, Expression<Func<TEntity, TProperty>> valueExpression)
			where TEntity : class => builder.Set<IEntityUpdateQueryBuilder<TEntity>, TEntity, TProperty>(fieldSelector, valueExpression);

		public static IEntityUpdateQueryBuilder<TEntity> Set<TEntity, TProperty>(this IEntityUpdateQueryBuilder<TEntity> builder, Expression<Func<TEntity, TProperty>> fieldSelector, Expression valueExpression)
			where TEntity : class => builder.Set<IEntityUpdateQueryBuilder<TEntity>, TEntity, TProperty>(fieldSelector, valueExpression);

		public static IEntityUpdateQueryBuilder<TEntity> Set<TEntity, TProperty>(this IEntityUpdateQueryBuilder<TEntity> builder, Expression<Func<TEntity, TProperty>> fieldSelector, IQueryBuilder subQuery)
			where TEntity : class => builder.Set<IEntityUpdateQueryBuilder<TEntity>, TEntity, TProperty>(fieldSelector, subQuery);

		public static IEntityUpdateQueryBuilder<TEntity> Set<TEntity, TProperty>(this IEntityUpdateQueryBuilder<TEntity> builder, Expression<Func<TEntity, TProperty>> fieldSelector, QueryExpression valueExpression)
			where TEntity : class => builder.Set<IEntityUpdateQueryBuilder<TEntity>, TEntity, TProperty>(fieldSelector, valueExpression);
	}
}
