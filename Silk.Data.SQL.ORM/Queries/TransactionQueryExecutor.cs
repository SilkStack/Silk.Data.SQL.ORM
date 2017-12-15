using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Silk.Data.SQL.Providers;

namespace Silk.Data.SQL.ORM.Queries
{
	public class TransactionQueryExecutor : IORMQueryExecutor
	{
		public List<object> ExecuteQueries(IEnumerable<ORMQuery> queries, IDataProvider dataProvider)
		{
			using (var transaction = dataProvider.CreateTransaction())
			{
				List<object> ret = null;
				foreach (var query in queries)
				{
					if (query.MapToType == null)
					{
						transaction.ExecuteNonQuery(query.Query);
					}
					else
					{
						using (var queryResult = transaction.ExecuteReader(query.Query))
						{
							var mapResult = query.MapResult(queryResult);
							if (query.IsQueryResult)
							{
								if (ret == null)
									ret = new List<object>();
								ret.Add(mapResult);
							}
						}
					}
				}
				transaction.Commit();
				return ret;
			}
		}

		public async Task<List<object>> ExecuteQueriesAsync(IEnumerable<ORMQuery> queries, IDataProvider dataProvider)
		{
			using (var transaction = await dataProvider.CreateTransactionAsync()
				.ConfigureAwait(false))
			{
				List<object> ret = null;
				foreach (var query in queries)
				{
					try
					{
						if (query.MapToType == null)
						{
							await transaction.ExecuteNonQueryAsync(query.Query)
								.ConfigureAwait(false);
						}
						else
						{
							using (var queryResult = await transaction.ExecuteReaderAsync(query.Query)
								.ConfigureAwait(false))
							{
								var mapResult = await query.MapResultAsync(queryResult)
									.ConfigureAwait(false);
								if (query.IsQueryResult)
								{
									if (ret == null)
										ret = new List<object>();
									ret.Add(mapResult);
								}
							}
						}
					}
					catch (Exception)
					{
						transaction.Rollback();
						throw;
					}
				}
				transaction.Commit();
				return ret;
			}
		}
	}
}
