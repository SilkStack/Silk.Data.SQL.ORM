using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Queries;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public abstract class Query
	{
		public QueryExpression QueryExpression { get; }
		public bool ProducesResultSet { get; }

		public Query(QueryExpression queryExpression, bool producesResultSet)
		{
			QueryExpression = queryExpression;
			ProducesResultSet = producesResultSet;
		}

		public virtual void ProcessResult(QueryResult queryResult) { }
		public virtual Task ProcessResultAsync(QueryResult queryResult) => Task.CompletedTask;
	}

	public class QueryNoResult : Query
	{
		public QueryNoResult(QueryExpression queryExpression)
			: base(queryExpression, false)
		{
		}
	}

	public class QueryInjectResult<T> : Query
		where T : class
	{
		public QueryInjectResult(QueryExpression queryExpression)
			: base(queryExpression, true)
		{
		}
	}

	public abstract class QueryWithResult<T> : Query
	{
		public QueryWithResult(QueryExpression queryExpression)
			: base(queryExpression, true)
		{
		}
	}

	public class QueryWithMappedResult<T> : QueryWithResult<T>
		where T : class
	{
		public QueryWithMappedResult(QueryExpression queryExpression)
			: base(queryExpression)
		{
		}
	}

	public class QueryWithScalarResult<T> : QueryWithResult<T>
		where T : struct
	{
		public QueryWithScalarResult(QueryExpression queryExpression)
			: base(queryExpression)
		{
		}
	}
}
