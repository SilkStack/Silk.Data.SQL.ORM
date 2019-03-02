using Silk.Data.SQL.Providers;
using Silk.Data.SQL.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public class SingleDeferableSelect<TEntity, TResult> : IDeferable<TResult>,
		IWhereQueryBuilder<TEntity>, IHavingQueryBuilder<TEntity>,
		IGroupByQueryBuilder<TEntity>, IOrderByQueryBuilder<TEntity>, IRangeQueryBuilder<TEntity>
		where TEntity : class
	{
		private readonly IEntitySelectQueryBuilder<TEntity> _queryBuilder;
		private readonly ISelectQueryBuilder _nonGenericBuilder;
		private readonly IResultReader<TResult> _resultReader;
		private readonly IDataProvider _dataProvider;

		public SingleDeferableSelect(IEntitySelectQueryBuilder<TEntity> queryBuilder, IDataProvider dataProvider,
			IResultReader<TResult> resultReader)
		{
			_queryBuilder = queryBuilder;
			_nonGenericBuilder = queryBuilder;
			_resultReader = resultReader;
			_dataProvider = dataProvider;
		}

		public IEntityConditionBuilder<TEntity> Where { get => _queryBuilder.Where; set => _queryBuilder.Where = value; }
		public IEntityConditionBuilder<TEntity> Having { get => _queryBuilder.Having; set => _queryBuilder.Having = value; }
		public IEntityGroupByBuilder<TEntity> GroupBy { get => _queryBuilder.GroupBy; set => _queryBuilder.GroupBy = value; }
		public IEntityOrderByBuilder<TEntity> OrderBy { get => _queryBuilder.OrderBy; set => _queryBuilder.OrderBy = value; }
		public IEntityRangeBuilder<TEntity> Range { get => _queryBuilder.Range; set => _queryBuilder.Range = value; }
		IConditionBuilder IWhereQueryBuilder.Where { get => _nonGenericBuilder.Where; set => _nonGenericBuilder.Where = value; }
		IConditionBuilder IHavingQueryBuilder.Having { get => _nonGenericBuilder.Having; set => _nonGenericBuilder.Having = value; }
		IGroupByBuilder IGroupByQueryBuilder.GroupBy { get => _nonGenericBuilder.GroupBy; set => _nonGenericBuilder.GroupBy = value; }
		IOrderByBuilder IOrderByQueryBuilder.OrderBy { get => _nonGenericBuilder.OrderBy; set => _nonGenericBuilder.OrderBy = value; }
		IRangeBuilder IRangeQueryBuilder.Range { get => _nonGenericBuilder.Range; set => _nonGenericBuilder.Range = value; }

		public IDeferred Defer(out DeferredResult<TResult> deferredResult)
		{
			var resultSource = new DeferredResultSource<TResult>();
			deferredResult = resultSource.DeferredResult;

			var result = new DeferredQuery(_dataProvider);
			result.Add(_queryBuilder.BuildQuery(), new SingleMappedResultProcessor(
				_resultReader,
				resultSource
				));

			return result;
		}

		public TResult Execute()
		{
			Defer(out var deferredResult).Execute();
			return deferredResult.Result;
		}

		public async Task<TResult> ExecuteAsync()
		{
			await Defer(out var deferredResult).ExecuteAsync();
			return deferredResult.Result;
		}

		private class SingleMappedResultProcessor : IQueryResultProcessor
		{
			private readonly IResultReader<TResult> _resultReader;
			private readonly DeferredResultSource<TResult> _resultSource;

			public SingleMappedResultProcessor(
				IResultReader<TResult> resultReader,
				DeferredResultSource<TResult> deferredResultSource
				)
			{
				_resultReader = resultReader;
				_resultSource = deferredResultSource;
			}

			public void HandleFailure()
			{
				if (_resultSource.DeferredResult.TaskHasRun)
					return;
				_resultSource.SetFailed();
			}

			public void ProcessResult(QueryResult queryResult)
			{
				if (!queryResult.HasRows || !queryResult.Read())
				{
					_resultSource.SetResult(default(TResult));
					return;
				}

				_resultSource.SetResult(_resultReader.Read(queryResult));
			}

			public async Task ProcessResultAsync(QueryResult queryResult)
			{
				if (!queryResult.HasRows || !await queryResult.ReadAsync())
				{
					_resultSource.SetResult(default(TResult));
					return;
				}

				_resultSource.SetResult(_resultReader.Read(queryResult));
			}
		}
	}

	public class MultipleDeferableSelect<TEntity, TResult> : IDeferable<List<TResult>>,
		IWhereQueryBuilder<TEntity>, IHavingQueryBuilder<TEntity>,
		IGroupByQueryBuilder<TEntity>, IOrderByQueryBuilder<TEntity>, IRangeQueryBuilder<TEntity>
		where TEntity : class
	{
		private readonly IEntitySelectQueryBuilder<TEntity> _queryBuilder;
		private readonly ISelectQueryBuilder _nonGenericBuilder;
		private readonly IResultReader<TResult> _resultReader;
		private readonly IDataProvider _dataProvider;

		public IEntityConditionBuilder<TEntity> Where { get => _queryBuilder.Where; set => _queryBuilder.Where = value; }
		public IEntityConditionBuilder<TEntity> Having { get => _queryBuilder.Having; set => _queryBuilder.Having = value; }
		public IEntityGroupByBuilder<TEntity> GroupBy { get => _queryBuilder.GroupBy; set => _queryBuilder.GroupBy = value; }
		public IEntityOrderByBuilder<TEntity> OrderBy { get => _queryBuilder.OrderBy; set => _queryBuilder.OrderBy = value; }
		public IEntityRangeBuilder<TEntity> Range { get => _queryBuilder.Range; set => _queryBuilder.Range = value; }
		IConditionBuilder IWhereQueryBuilder.Where { get => _nonGenericBuilder.Where; set => _nonGenericBuilder.Where = value; }
		IConditionBuilder IHavingQueryBuilder.Having { get => _nonGenericBuilder.Having; set => _nonGenericBuilder.Having = value; }
		IGroupByBuilder IGroupByQueryBuilder.GroupBy { get => _nonGenericBuilder.GroupBy; set => _nonGenericBuilder.GroupBy = value; }
		IOrderByBuilder IOrderByQueryBuilder.OrderBy { get => _nonGenericBuilder.OrderBy; set => _nonGenericBuilder.OrderBy = value; }
		IRangeBuilder IRangeQueryBuilder.Range { get => _nonGenericBuilder.Range; set => _nonGenericBuilder.Range = value; }

		public MultipleDeferableSelect(IEntitySelectQueryBuilder<TEntity> queryBuilder, IDataProvider dataProvider,
			IResultReader<TResult> resultReader)
		{
			_queryBuilder = queryBuilder;
			_nonGenericBuilder = queryBuilder;
			_resultReader = resultReader;
			_dataProvider = dataProvider;
		}

		public IDeferred Defer(out DeferredResult<List<TResult>> deferredResult)
		{
			var resultSource = new DeferredResultSource<List<TResult>>();
			deferredResult = resultSource.DeferredResult;

			var result = new DeferredQuery(_dataProvider);
			result.Add(_queryBuilder.BuildQuery(), new ManyMappedResultProcessor(
				_resultReader,
				resultSource
				));

			return result;
		}

		public List<TResult> Execute()
		{
			Defer(out var deferredResult).Execute();
			return deferredResult.Result;
		}

		public async Task<List<TResult>> ExecuteAsync()
		{
			await Defer(out var deferredResult).ExecuteAsync();
			return deferredResult.Result;
		}

		private class ManyMappedResultProcessor : IQueryResultProcessor
		{
			private readonly IResultReader<TResult> _resultReader;
			private readonly DeferredResultSource<List<TResult>> _resultSource;

			public ManyMappedResultProcessor(
				IResultReader<TResult> resultReader,
				DeferredResultSource<List<TResult>> deferredResultSource
				)
			{
				_resultReader = resultReader;
				_resultSource = deferredResultSource;
			}

			public void HandleFailure()
			{
				if (_resultSource.DeferredResult.TaskHasRun)
					return;
				_resultSource.SetFailed();
			}

			public void ProcessResult(QueryResult queryResult)
			{
				var result = new List<TResult>();
				while (queryResult.Read())
					result.Add(_resultReader.Read(queryResult));
				_resultSource.SetResult(result);
			}

			public async Task ProcessResultAsync(QueryResult queryResult)
			{
				var result = new List<TResult>();
				while (await queryResult.ReadAsync())
					result.Add(_resultReader.Read(queryResult));
				_resultSource.SetResult(result);
			}
		}
	}
}
