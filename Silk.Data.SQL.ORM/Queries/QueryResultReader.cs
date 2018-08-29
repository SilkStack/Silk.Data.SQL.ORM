using Silk.Data.Modelling;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Queries
{
	public class QueryResultReader : IModelReadWriter
	{
		private static readonly Dictionary<Type, Delegate> _typeReaders =
			new Dictionary<Type, Delegate>()
			{
				{ typeof(bool), new Func<QueryResult, int, bool>((q,o) => q.GetBoolean(o)) },
				{ typeof(byte), new Func<QueryResult, int, byte>((q,o) => q.GetByte(o)) },
				{ typeof(short), new Func<QueryResult, int, short>((q,o) => q.GetInt16(o)) },
				{ typeof(int), new Func<QueryResult, int, int>((q,o) => q.GetInt32(o)) },
				{ typeof(long), new Func<QueryResult, int, long>((q,o) => q.GetInt64(o)) },
				{ typeof(float), new Func<QueryResult, int, float>((q,o) => q.GetFloat(o)) },
				{ typeof(double), new Func<QueryResult, int, double>((q,o) => q.GetDouble(o)) },
				{ typeof(decimal), new Func<QueryResult, int, decimal>((q,o) => q.GetDecimal(o)) },
				{ typeof(string), new Func<QueryResult, int, string>((q,o) => q.GetString(o)) },
				{ typeof(Guid), new Func<QueryResult, int, Guid>((q,o) => q.GetGuid(o)) },
				{ typeof(DateTime), new Func<QueryResult, int, DateTime>((q,o) => q.GetDateTime(o)) }
			};

		public QueryResult QueryResult { get; }

		public IModel Model => throw new NotImplementedException();

		public QueryResultReader(QueryResult queryResult)
		{
			QueryResult = queryResult;
		}

		public T ReadField<T>(string[] path, int offset)
		{
			if (!_typeReaders.TryGetValue(typeof(T), out var @delegate))
				throw new Exception("Unsupported data type.");

			var ord = QueryResult.GetOrdinal(path[0]);
			if (QueryResult.IsDBNull(ord))
				return default(T);

			var readFunc = @delegate as Func<QueryResult, int, T>;
			return readFunc(QueryResult, ord);
		}

		public void WriteField<T>(string[] path, int offset, T value)
		{
			throw new NotImplementedException();
		}
	}
}
