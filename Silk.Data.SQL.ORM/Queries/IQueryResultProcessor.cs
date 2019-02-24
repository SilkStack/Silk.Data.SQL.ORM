using Silk.Data.SQL.Queries;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public interface IQueryResultProcessor
	{
		void ProcessResult(QueryResult queryResult);
		Task ProcessResultAsync(QueryResult queryResult);
		void HandleFailure();
	}
}
