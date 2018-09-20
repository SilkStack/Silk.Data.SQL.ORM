using Silk.Data.SQL.ORM.Queries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Silk.Data.SQL.ORM
{
	public static class QueryExtensions
	{
		public static Query WithResult<T>(this QueryWithMappedResult<T> query, Action<ICollection<T>> callback)
			where T : class
		{
			return new WithMappedResultQuery<T>(query, callback);
		}

		public static Query WithResult<T>(this QueryWithScalarResult<T> query, Action<ICollection<T>> callback)
			where T : struct
		{
			return new WithScalarResultQuery<T>(query, callback);
		}

		public static Query WithResult<T1, T2>(this QueryWithTupleResult<T1, T2> query, Action<ICollection<(T1, T2)>> callback)
			where T1 : class
			where T2 : class
		{
			return new WithTupleResultQuery<T1, T2>(query, callback);
		}

		public static Query WithFirstResult<T>(this QueryWithMappedResult<T> query, Action<T> callback)
			where T : class
		{
			return new WithFirstMappedResultQuery<T>(query, callback);
		}

		public static Query WithFirstResult<T>(this QueryWithScalarResult<T> query, Action<T> callback)
			where T : struct
		{
			return new WithFirstScalarResultQuery<T>(query, callback);
		}

		public static Query WithFirstResult<T1, T2>(this QueryWithTupleResult<T1, T2> query, Action<(T1, T2)> callback)
			where T1 : class
			where T2 : class
		{
			return new WithFirstTupleResultQuery<T1, T2>(query, callback);
		}
	}
}
