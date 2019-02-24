using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Providers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public class DeferredQuery : IDeferredBatch
	{
		private readonly List<QueryInfo> _queries = new List<QueryInfo>();

		public IQueryProvider QueryProvider { get; }

		public DeferredQuery(IQueryProvider queryProvider)
		{
			QueryProvider = queryProvider;
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
			using (var queryResult = QueryProvider.ExecuteReader(ComposeQuery()))
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

		public async Task ExecuteAsync()
		{
			using (var queryResult = await QueryProvider.ExecuteReaderAsync(ComposeQuery()))
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

		public bool TryMerge(IDeferredBatch batch)
		{
			var deferredQuery = batch as DeferredQuery;
			if (batch == null || !ReferenceEquals(deferredQuery.QueryProvider, QueryProvider))
				return false;

			_queries.AddRange(deferredQuery._queries);

			return true;
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
