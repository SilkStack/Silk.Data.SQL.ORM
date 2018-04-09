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
	}
}
