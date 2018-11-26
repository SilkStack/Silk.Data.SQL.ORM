using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
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
				if (queryResult.Read())
					_resultMapper.Inject(entity, queryResult);
				queryResult.NextResult();
			}
		}

		public override async Task ProcessResultAsync(QueryResult queryResult)
		{
			foreach (var entity in _entities)
			{
				if (await queryResult.ReadAsync())
					_resultMapper.Inject(entity, queryResult);
				await queryResult.NextResultAsync();
			}
		}
	}

	public abstract class QueryWithResult<T> : Query
	{
		public ICollection<T> Result { get; protected set; }

		public QueryWithResult(QueryExpression queryExpression)
			: base(queryExpression, true)
		{
		}
	}

	public class QueryWithMappedResult<T> : QueryWithResult<T>
		where T : class
	{
		private readonly ObjectResultMapper<T> _resultMapper;

		public QueryWithMappedResult(QueryExpression queryExpression, ObjectResultMapper<T> resultMapper)
			: base(queryExpression)
		{
			_resultMapper = resultMapper;
		}

		public override void ProcessResult(QueryResult queryResult)
		{
			Result = _resultMapper.MapAll(queryResult);
		}

		public override async Task ProcessResultAsync(QueryResult queryResult)
		{
			Result = await _resultMapper.MapAllAsync(queryResult);
		}
	}

	public class QueryWithScalarResult<T> : QueryWithResult<T>
		where T : struct
	{
		private readonly ValueResultMapper<T> _resultMapper;

		public QueryWithScalarResult(QueryExpression queryExpression, ValueResultMapper<T> resultMapper)
			: base(queryExpression)
		{
			_resultMapper = resultMapper;
		}

		public override void ProcessResult(QueryResult queryResult)
		{
			Result = _resultMapper.ReadAll(queryResult);
		}

		public override async Task ProcessResultAsync(QueryResult queryResult)
		{
			Result = await _resultMapper.ReadAllAsync(queryResult);
		}
	}

	public class QueryWithTupleResult<T1, T2> : QueryWithResult<(T1, T2)>
		where T1 : class
		where T2 : class
	{
		private readonly TupleResultMapper<T1, T2> _resultMapper;

		public QueryWithTupleResult(QueryExpression queryExpression, TupleResultMapper<T1, T2> resultMapper)
			: base(queryExpression)
		{
			_resultMapper = resultMapper;
		}

		public override void ProcessResult(QueryResult queryResult)
		{
			Result = _resultMapper.MapAll(queryResult);
		}

		public override async Task ProcessResultAsync(QueryResult queryResult)
		{
			Result = await _resultMapper.MapAllAsync(queryResult);
		}
	}

	public class WithMappedResultQuery<T> : Query
		where T : class
	{
		private readonly QueryWithMappedResult<T> _query;
		private readonly Action<ICollection<T>> _callback;

		public WithMappedResultQuery(QueryWithMappedResult<T> query, Action<ICollection<T>> callback)
			: base(query.QueryExpression, query.ProducesResultSet)
		{
			_query = query;
			_callback = callback;
		}

		public override void ProcessResult(QueryResult queryResult)
		{
			_query.ProcessResult(queryResult);
			_callback?.Invoke(_query.Result);
		}

		public override async Task ProcessResultAsync(QueryResult queryResult)
		{
			await _query.ProcessResultAsync(queryResult);
			_callback?.Invoke(_query.Result);
		}
	}

	public class WithScalarResultQuery<T> : Query
		where T : struct
	{
		private readonly QueryWithScalarResult<T> _query;
		private readonly Action<ICollection<T>> _callback;

		public WithScalarResultQuery(QueryWithScalarResult<T> query, Action<ICollection<T>> callback)
			: base(query.QueryExpression, query.ProducesResultSet)
		{
			_query = query;
			_callback = callback;
		}

		public override void ProcessResult(QueryResult queryResult)
		{
			_query.ProcessResult(queryResult);
			_callback?.Invoke(_query.Result);
		}

		public override async Task ProcessResultAsync(QueryResult queryResult)
		{
			await _query.ProcessResultAsync(queryResult);
			_callback?.Invoke(_query.Result);
		}
	}

	public class WithTupleResultQuery<T1, T2> : Query
		where T1 : class
		where T2 : class
	{
		private readonly QueryWithTupleResult<T1, T2> _query;
		private readonly Action<ICollection<(T1, T2)>> _callback;

		public WithTupleResultQuery(QueryWithTupleResult<T1, T2> query, Action<ICollection<(T1, T2)>> callback)
			: base(query.QueryExpression, query.ProducesResultSet)
		{
			_query = query;
			_callback = callback;
		}

		public override void ProcessResult(QueryResult queryResult)
		{
			_query.ProcessResult(queryResult);
			_callback?.Invoke(_query.Result);
		}

		public override async Task ProcessResultAsync(QueryResult queryResult)
		{
			await _query.ProcessResultAsync(queryResult);
			_callback?.Invoke(_query.Result);
		}
	}

	public class WithFirstMappedResultQuery<T> : Query
		where T : class
	{
		private readonly QueryWithMappedResult<T> _query;
		private readonly Action<T> _callback;

		public WithFirstMappedResultQuery(QueryWithMappedResult<T> query, Action<T> callback)
			: base(query.QueryExpression, query.ProducesResultSet)
		{
			_query = query;
			_callback = callback;
		}

		public override void ProcessResult(QueryResult queryResult)
		{
			_query.ProcessResult(queryResult);
			if (_query.Result.Count > 0)
				_callback?.Invoke(_query.Result.First());
		}

		public override async Task ProcessResultAsync(QueryResult queryResult)
		{
			await _query.ProcessResultAsync(queryResult);
			if (_query.Result.Count > 0)
				_callback?.Invoke(_query.Result.First());
		}
	}

	public class WithFirstScalarResultQuery<T> : Query
		where T : struct
	{
		private readonly QueryWithScalarResult<T> _query;
		private readonly Action<T> _callback;

		public WithFirstScalarResultQuery(QueryWithScalarResult<T> query, Action<T> callback)
			: base(query.QueryExpression, query.ProducesResultSet)
		{
			_query = query;
			_callback = callback;
		}

		public override void ProcessResult(QueryResult queryResult)
		{
			_query.ProcessResult(queryResult);
			if (_query.Result.Count > 0)
				_callback?.Invoke(_query.Result.First());
		}

		public override async Task ProcessResultAsync(QueryResult queryResult)
		{
			await _query.ProcessResultAsync(queryResult);
			if (_query.Result.Count > 0)
				_callback?.Invoke(_query.Result.First());
		}
	}

	public class WithFirstTupleResultQuery<T1, T2> : Query
		where T1 : class
		where T2 : class
	{
		private readonly QueryWithTupleResult<T1, T2> _query;
		private readonly Action<(T1, T2)> _callback;

		public WithFirstTupleResultQuery(QueryWithTupleResult<T1, T2> query, Action<(T1, T2)> callback)
			: base(query.QueryExpression, query.ProducesResultSet)
		{
			_query = query;
			_callback = callback;
		}

		public override void ProcessResult(QueryResult queryResult)
		{
			_query.ProcessResult(queryResult);
			if (_query.Result.Count > 0)
				_callback?.Invoke(_query.Result.First());
		}

		public override async Task ProcessResultAsync(QueryResult queryResult)
		{
			await _query.ProcessResultAsync(queryResult);
			if (_query.Result.Count > 0)
				_callback?.Invoke(_query.Result.First());
		}
	}
}
