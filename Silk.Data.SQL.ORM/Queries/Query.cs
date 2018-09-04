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
		private readonly ObjectResultMapper<T> _resultMapper;
		private readonly T[] _entities;

		public QueryInjectResult(QueryExpression queryExpression, ObjectResultMapper<T> resultMapper,
			T[] entities)
			: base(queryExpression, true)
		{
			_resultMapper = resultMapper;
			_entities = entities;
		}

		public override void ProcessResult(QueryResult queryResult)
		{
			foreach (var entity in _entities)
			{
				queryResult.Read();
				_resultMapper.Inject(entity, queryResult);
				queryResult.NextResult();
			}
		}

		public override async Task ProcessResultAsync(QueryResult queryResult)
		{
			foreach (var entity in _entities)
			{
				await queryResult.ReadAsync();
				_resultMapper.Inject(entity, queryResult);
				await queryResult.NextResultAsync();
			}
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
