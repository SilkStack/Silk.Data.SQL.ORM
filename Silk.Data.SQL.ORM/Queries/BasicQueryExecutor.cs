using Silk.Data.SQL.Providers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public class BasicQueryExecutor : IORMQueryExecutor
	{
		public List<object> ExecuteQueries(IEnumerable<ORMQuery> queries, IDataProvider dataProvider)
		{
			List<object> ret = null;
			foreach (var query in queries)
			{
				if (query.MapToType == null)
				{
					dataProvider.ExecuteNonQuery(query.Query);
				}
				else
				{
					using (var queryResult = dataProvider.ExecuteReader(query.Query))
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
			return ret;
		}

		public async Task<List<object>> ExecuteQueriesAsync(IEnumerable<ORMQuery> queries, IDataProvider dataProvider)
		{
			List<object> ret = null;
			foreach (var query in queries)
			{
				if (query.MapToType == null)
				{
					await dataProvider.ExecuteNonQueryAsync(query.Query)
						.ConfigureAwait(false);
				}
				else
				{
					using (var queryResult = await dataProvider.ExecuteReaderAsync(query.Query)
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
			return ret;
		}
	}
}
