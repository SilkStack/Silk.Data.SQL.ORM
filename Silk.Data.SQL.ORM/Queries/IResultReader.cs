using Silk.Data.SQL.Queries;

namespace Silk.Data.SQL.ORM.Queries
{
	public interface IResultReader
	{
	}

	public interface IResultReader<T> : IResultReader
	{
		T Read(QueryResult queryResult);
	}
}
