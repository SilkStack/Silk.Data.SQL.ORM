using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Queries;
using System;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public class QueryWithDelegate
	{
		public QueryExpression Query { get; }
		public Action<QueryResult> Delegate { get; }
		public Func<QueryResult, Task> AsyncDelegate { get; }

		public QueryWithDelegate(QueryExpression query, Action<QueryResult> @delegate = null,
			Func<QueryResult, Task> asyncDelegate = null)
		{
			Query = query;
			Delegate = @delegate;
			AsyncDelegate = asyncDelegate;
		}
	}
}
