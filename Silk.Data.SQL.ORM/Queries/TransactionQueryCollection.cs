using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public class TransactionQueryCollection : ExecutableQueryCollection
	{
		public TransactionQueryCollection(params QueryWithDelegate[] queryExpressions)
			: base(queryExpressions)
		{
		}

		public TransactionQueryCollection(IEnumerable<QueryWithDelegate> queryExpressions)
			: base(queryExpressions)
		{
		}

		public new void Execute(IDataProvider dataProvider)
		{
			using (var queryResult = dataProvider
				.ExecuteReader(QueryExpression.Transaction(Queries.Select(q => q.Query))))
			{
				foreach (var query in Queries)
				{
					if (query.Query is SelectExpression)
					{
						if (!queryResult.NextResult())
							throw new Exception("Failed to move to query result.");
						query.Delegate?.Invoke(queryResult);
					}
				}
			}
		}

		public new async Task ExecuteAsync(IDataProvider dataProvider)
		{
			using (var queryResult = await dataProvider
				.ExecuteReaderAsync(QueryExpression.Transaction(Queries.Select(q => q.Query)))
				.ConfigureAwait(false))
			{
				foreach (var query in Queries)
				{
					if (query.Query is SelectExpression)
					{
						if (!await queryResult.NextResultAsync()
							.ConfigureAwait(false))
							throw new Exception("Failed to move to query result.");
						if (query.AsyncDelegate != null)
							await query.AsyncDelegate(queryResult).ConfigureAwait(false);
					}
				}
			}
		}
	}

	public class TransactionQueryCollection<TQueryResult> : TransactionQueryCollection
		where TQueryResult : new()
	{
		public TransactionQueryCollection(params QueryWithDelegate[] queryExpressions)
			: base(queryExpressions)
		{
		}

		public TransactionQueryCollection(IEnumerable<QueryWithDelegate> queryExpressions)
			: base(queryExpressions)
		{
		}

		public new ICollection<TQueryResult> Execute(IDataProvider dataProvider)
		{
			ICollection<TQueryResult> ret = null;

			using (var queryResult = dataProvider
				.ExecuteReader(QueryExpression.Transaction(Queries.Select(q => q.Query))))
			{
				foreach (var query in Queries)
				{
					if (query.Query is SelectExpression)
					{
						if (!queryResult.NextResult())
							throw new Exception("Failed to move to query result.");
						query.Delegate?.Invoke(queryResult);
						if (query.AssignsResults)
							ret = query.Results as ICollection<TQueryResult>;
					}
				}
			}

			return ret;
		}

		public new async Task<ICollection<TQueryResult>> ExecuteAsync(IDataProvider dataProvider)
		{
			ICollection<TQueryResult> ret = null;

			using (var queryResult = await dataProvider
				.ExecuteReaderAsync(QueryExpression.Transaction(Queries.Select(q => q.Query)))
				.ConfigureAwait(false))
			{
				foreach (var query in Queries)
				{
					if (query.Query is SelectExpression)
					{
						if (!await queryResult.NextResultAsync()
							.ConfigureAwait(false))
							throw new Exception("Failed to move to query result.");
						if (query.AsyncDelegate != null)
							await query.AsyncDelegate(queryResult).ConfigureAwait(false);
						if (query.AssignsResults)
							ret = query.Results as ICollection<TQueryResult>;
					}
				}
			}

			return ret;
		}
	}

	public class TransactionQueryCollection<TQueryResult1, TQueryResult2> : ExecutableQueryCollection
		where TQueryResult1 : new()
		where TQueryResult2 : new()
	{
		public TransactionQueryCollection(params QueryWithDelegate[] queryExpressions)
			: base(queryExpressions)
		{
		}

		public TransactionQueryCollection(IEnumerable<QueryWithDelegate> queryExpressions)
			: base(queryExpressions)
		{
		}

		public new(ICollection<TQueryResult1> Result1, ICollection<TQueryResult2> Result2) Execute(IDataProvider dataProvider)
		{
			ICollection<TQueryResult1> result1 = null;
			ICollection<TQueryResult2> result2 = null;

			using (var queryResult = dataProvider
				.ExecuteReader(QueryExpression.Transaction(Queries.Select(q => q.Query))))
			{
				foreach (var query in Queries)
				{
					if (query.Query is SelectExpression)
					{
						if (!queryResult.NextResult())
							throw new Exception("Failed to move to query result.");
						query.Delegate?.Invoke(queryResult);
						if (result1 == null && query.AssignsResults)
							result1 = query.Results as ICollection<TQueryResult1>;
						else if (result2 == null && query.AssignsResults)
							result2 = query.Results as ICollection<TQueryResult2>;
					}
				}
			}

			return (result1, result2);
		}

		public new async Task<(ICollection<TQueryResult1> Result1, ICollection<TQueryResult2> Result2)> ExecuteAsync(IDataProvider dataProvider)
		{
			ICollection<TQueryResult1> result1 = null;
			ICollection<TQueryResult2> result2 = null;

			using (var queryResult = await dataProvider
				.ExecuteReaderAsync(QueryExpression.Transaction(Queries.Select(q => q.Query)))
				.ConfigureAwait(false))
			{
				foreach (var query in Queries)
				{
					if (query.Query is SelectExpression)
					{
						if (!await queryResult.NextResultAsync()
							.ConfigureAwait(false))
							throw new Exception("Failed to move to query result.");
						if (query.AsyncDelegate != null)
							await query.AsyncDelegate(queryResult).ConfigureAwait(false);
						if (result1 == null && query.AssignsResults)
							result1 = query.Results as ICollection<TQueryResult1>;
						else if (result2 == null && query.AssignsResults)
							result2 = query.Results as ICollection<TQueryResult2>;
					}
				}
			}

			return (result1, result2);
		}
	}
}
