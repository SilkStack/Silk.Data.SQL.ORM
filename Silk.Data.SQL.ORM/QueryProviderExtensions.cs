using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Queries.Expressions;
using System.Linq;
using System.Threading.Tasks;
using IQueryProvider = Silk.Data.SQL.Providers.IQueryProvider;

namespace Silk.Data.SQL.ORM
{
	public static class QueryProviderExtensions
	{
		public static void Execute(this IQueryProvider dataProvider, params Query[] queries)
		{
			var compositeQuery = new CompositeQueryExpression(
				queries.Select(q => q.QueryExpression)
				);

			if (!queries.Any(q => q.ProducesResultSet))
			{
				dataProvider.ExecuteNonQuery(compositeQuery);
				return;
			}

			using (var queryResult = dataProvider.ExecuteReader(compositeQuery))
			{
				foreach (var query in queries.Where(q => q.ProducesResultSet))
				{
					query.ProcessResult(queryResult);
				}
			}
		}

		public static async Task ExecuteAsync(this IQueryProvider dataProvider, params Query[] queries)
		{
			var compositeQuery = new CompositeQueryExpression(
				queries.Select(q => q.QueryExpression)
				);

			if (!queries.Any(q => q.ProducesResultSet))
			{
				await dataProvider.ExecuteNonQueryAsync(compositeQuery);
				return;
			}

			using (var queryResult = await dataProvider.ExecuteReaderAsync(compositeQuery))
			{
				foreach (var query in queries.Where(q => q.ProducesResultSet))
				{
					await query.ProcessResultAsync(queryResult);
				}
			}
		}
	}
}
