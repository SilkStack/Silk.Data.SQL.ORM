using Silk.Data.SQL.Providers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public interface IORMQueryExecutor
	{
		List<object> ExecuteQueries(IEnumerable<ORMQuery> queries, IDataProvider dataProvider);
		Task<List<object>> ExecuteQueriesAsync(IEnumerable<ORMQuery> queries, IDataProvider dataProvider);
	}
}
