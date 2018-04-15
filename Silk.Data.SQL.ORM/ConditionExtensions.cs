using Silk.Data.SQL.Expressions;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM
{
	public static class ConditionExtensions
	{
		public static ConditionBuilder<T> Condition<T>(this IDatabase<T> database)
			where T : class
		{
			return new ConditionBuilder<T>(database.EntityModel);
		}

		public static ConditionBuilder<T> Condition<T>(this IDatabase<T> database, Condition condition)
			where T : class
		{
			return database.Condition().And(condition);
		}

		public static ConditionBuilder<T> Condition<T>(this IDatabase<T> database, Expression<Func<T, bool>> condition)
			where T : class
		{
			return database.Condition().And(condition);
		}

		public static GroupByBuilder<T> GroupBy<T>(this IDatabase<T> database)
			where T : class
		{
			return new GroupByBuilder<T>(database.EntityModel);
		}

		public static GroupByBuilder<T> GroupBy<T>(this IDatabase<T> database, QueryExpression queryExpression)
			where T : class
		{
			return database.GroupBy().ThenBy(queryExpression);
		}

		public static GroupByBuilder<T> GroupBy<T, TProperty>(this IDatabase<T> database, Expression<Func<T, TProperty>> groupBy)
			where T : class
		{
			return database.GroupBy().ThenBy(groupBy);
		}

		public static OrderByBuilder<T> OrderBy<T>(this IDatabase<T> database)
			where T : class
		{
			return new OrderByBuilder<T>(database.EntityModel);
		}

		public static OrderByBuilder<T> OrderBy<T>(this IDatabase<T> database, QueryExpression queryExpression)
			where T : class
		{
			return database.OrderBy().ThenBy(queryExpression);
		}

		public static OrderByBuilder<T> OrderBy<T, TProperty>(this IDatabase<T> database, Expression<Func<T, TProperty>> orderBy)
			where T : class
		{
			return database.OrderBy().ThenBy(orderBy);
		}

		public static OrderByBuilder<T> OrderByDescending<T>(this IDatabase<T> database, QueryExpression queryExpression)
			where T : class
		{
			return database.OrderBy().ThenByDescending(queryExpression);
		}

		public static OrderByBuilder<T> OrderByDescending<T, TProperty>(this IDatabase<T> database, Expression<Func<T, TProperty>> orderBy)
			where T : class
		{
			return database.OrderBy().ThenByDescending(orderBy);
		}
	}
}
