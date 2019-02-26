using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Providers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public class DeferredQuery : IDeferredBatch
	{
		private readonly List<QueryInfo> _queries = new List<QueryInfo>();
		private QueryTransactionController _transactionController;

		public IDataProvider DataProvider { get; }

		public DeferredQuery(IDataProvider dataProvider)
		{
			DataProvider = dataProvider;
		}

		public void Add(QueryExpression query, bool producesResultSet = false)
		{
			_queries.Add(new QueryInfo(query, producesResultSet));
		}

		public void Add(QueryExpression query, IQueryResultProcessor resultSetProcessor)
		{
			_queries.Add(new QueryInfo(query, resultSetProcessor));
		}

		private CompositeQueryExpression ComposeQuery()
		{
			var compositeQuery = new CompositeQueryExpression();
			foreach (var query in _queries)
				compositeQuery.Queries.Add(query.Query);
			return compositeQuery;
		}

		public void Execute()
		{
			try
			{
				IQueryProvider queryProvider = _transactionController?.Transaction;
				if (queryProvider == null)
					queryProvider = DataProvider;
				using (var queryResult = queryProvider.ExecuteReader(ComposeQuery()))
				{
					foreach (var query in _queries)
					{
						if (query.ProducesResultSet)
						{
							query.ResultSetProcessor?.ProcessResult(queryResult);
							queryResult.NextResult();
						}
					}
				}
			}
			catch
			{
				foreach (var query in _queries)
					query.ResultSetProcessor?.HandleFailure();
				throw;
			}
		}

		public async Task ExecuteAsync()
		{
			try
			{
				IQueryProvider queryProvider = _transactionController?.Transaction;
				if (queryProvider == null)
					queryProvider = DataProvider;
				using (var queryResult = await queryProvider.ExecuteReaderAsync(ComposeQuery()))
				{
					foreach (var query in _queries)
					{
						if (query.ProducesResultSet)
						{
							if (query.ResultSetProcessor != null)
								await query.ResultSetProcessor.ProcessResultAsync(queryResult);
							await queryResult.NextResultAsync();
						}
					}
				}
			}
			catch
			{
				foreach (var query in _queries)
					query.ResultSetProcessor?.HandleFailure();
				throw;
			}
		}

		public bool TryMerge(IDeferredBatch batch)
		{
			var deferredQuery = batch as DeferredQuery;
			if (batch == null || !ReferenceEquals(deferredQuery.DataProvider, DataProvider))
				return false;

			_queries.AddRange(deferredQuery._queries);

			return true;
		}

		public ITransactionController GetTransactionControllerImplementation()
			=> new QueryTransactionController(DataProvider);

		public void SetSharedTransactionController(ITransactionController transactionController)
		{
			_transactionController = transactionController as QueryTransactionController;
		}

		private class QueryInfo
		{
			public QueryExpression Query { get; }
			public bool ProducesResultSet { get; }
			public IQueryResultProcessor ResultSetProcessor { get; }

			public QueryInfo(QueryExpression query, bool producesResultSet)
			{
				Query = query;
				ProducesResultSet = producesResultSet;
			}

			public QueryInfo(QueryExpression query, IQueryResultProcessor resultProcessor) :
				this(query, true)
			{
				ResultSetProcessor = resultProcessor;
			}
		}
	}
}
