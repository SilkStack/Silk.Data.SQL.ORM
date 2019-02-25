using Silk.Data.SQL.Queries;

namespace Silk.Data.SQL.ORM.Queries
{
	public class ValueReader<T> : IResultReader<T>
	{
		private readonly string _aliasName;

		public ValueReader(string aliasName)
		{
			_aliasName = aliasName;
		}

		public T Read(QueryResult queryResult)
		{
			var ord = queryResult.GetOrdinal(_aliasName);
			if (queryResult.IsDBNull(ord))
				return default(T);

			if (typeof(T).IsEnum)
				return (T)(object)QueryTypeReaders.GetTypeReader<int>()(queryResult, ord);
			return QueryTypeReaders.GetTypeReader<T>()(queryResult, ord);
		}
	}
}
