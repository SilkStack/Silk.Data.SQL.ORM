using Silk.Data.SQL.Providers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public abstract class QueryCollection
	{
		protected List<ORMQuery> Queries { get; } = new List<ORMQuery>();
	}

	public class ExecutableQueryCollection : QueryCollection
	{
		public ExecutableQueryCollection(IEnumerable<ORMQuery> queryExpressions)
		{
			Queries.AddRange(queryExpressions);
		}

		public ExecutableQueryCollection(params ORMQuery[] queryExpressions)
		{
			Queries.AddRange(queryExpressions);
		}

		public TransactionQueryCollection AsTransaction()
		{
			return new TransactionQueryCollection(Queries.ToArray());
		}

		public void Execute(IDataProvider dataProvider)
		{
			foreach (var query in Queries)
			{
				if (query.MapToType == null)
				{
					dataProvider.ExecuteNonQuery(query.Query);
				}
				else
				{
					using (var queryResult = dataProvider.ExecuteReader(query.Query))
					{
						query.MapResult(queryResult);
					}
				}
			}
		}

		public async Task ExecuteAsync(IDataProvider dataProvider)
		{
			foreach (var query in Queries)
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
						await query.MapResultAsync(queryResult)
							.ConfigureAwait(false);
					}
				}
			}
		}
	}
}
