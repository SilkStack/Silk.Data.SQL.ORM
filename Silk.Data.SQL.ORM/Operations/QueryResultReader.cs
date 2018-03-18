using Silk.Data.Modelling;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Operations
{
	public class QueryResultReader : IModelReadWriter
	{
		private static readonly Dictionary<Type, Func<QueryResult, int, object>> _typeReaders =
			new Dictionary<Type, Func<QueryResult, int, object>>()
			{
				{ typeof(bool), (q,o) => q.GetBoolean(o) },
				{ typeof(byte), (q,o) => q.GetByte(o) },
				{ typeof(short), (q,o) => q.GetInt16(o) },
				{ typeof(int), (q,o) => q.GetInt32(o) },
				{ typeof(long), (q,o) => q.GetInt64(o) },
				{ typeof(float), (q,o) => q.GetFloat(o) },
				{ typeof(double), (q,o) => q.GetDouble(o) },
				{ typeof(decimal), (q,o) => q.GetDecimal(o) },
				{ typeof(string), (q,o) => q.GetString(o) },
				{ typeof(Guid), (q,o) => q.GetGuid(o) },
				{ typeof(DateTime), (q,o) => q.GetDateTime(o) },
			};

		private readonly QueryResult _queryResult;
		public IModel Model { get; }

		public QueryResultReader(IModel model, QueryResult queryResult)
		{
			Model = model;
			_queryResult = queryResult;
		}

		public T ReadField<T>(string[] path, int offset)
		{
			if (!_typeReaders.TryGetValue(typeof(T), out var readFunc))
				return default(T);

			var fieldAlias = string.Join("_", path);
			var ord = _queryResult.GetOrdinal(fieldAlias);
			return (T)readFunc(_queryResult, ord);
		}

		public void WriteField<T>(string[] path, int offset, T value)
		{
			throw new System.NotImplementedException();
		}
	}
}
