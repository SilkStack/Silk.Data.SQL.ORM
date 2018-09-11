﻿using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Queries;
using System.Collections.Generic;
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
}
