using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Queries
{
	public class QueryGraphReader : IGraphReader<EntityModel, EntityField>
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
				{ typeof(DateTime), new Func<QueryResult, int, DateTime>((q,o) => q.GetDateTime(o)) },

				{ typeof(bool?), new Func<QueryResult, int, bool?>((q,o) => q.GetBoolean(o)) },
				{ typeof(byte?), new Func<QueryResult, int, byte?>((q,o) => q.GetByte(o)) },
				{ typeof(short?), new Func<QueryResult, int, short?>((q,o) => q.GetInt16(o)) },
				{ typeof(int?), new Func<QueryResult, int, int?>((q,o) => q.GetInt32(o)) },
				{ typeof(long?), new Func<QueryResult, int, long?>((q,o) => q.GetInt64(o)) },
				{ typeof(float?), new Func<QueryResult, int, float?>((q,o) => q.GetFloat(o)) },
				{ typeof(double?), new Func<QueryResult, int, double?>((q,o) => q.GetDouble(o)) },
				{ typeof(decimal?), new Func<QueryResult, int, decimal?>((q,o) => q.GetDecimal(o)) },
				{ typeof(Guid?), new Func<QueryResult, int, Guid?>((q,o) => q.GetGuid(o)) },
				{ typeof(DateTime?), new Func<QueryResult, int, DateTime?>((q,o) => q.GetDateTime(o)) }
			};

		private readonly QueryResult _queryResult;

		public QueryGraphReader(QueryResult queryResult)
		{
			_queryResult = queryResult;
		}

		public bool CheckContainer(IFieldPath<EntityModel, EntityField> fieldPath)
			=> true;

		public bool CheckPath(IFieldPath<EntityModel, EntityField> fieldPath)
		{
			//  technically we could fetch the names for each column in the query result and check the field is actually present
			//  but do we need to in reality? the generated queries shouldn't fault
			var ord = _queryResult.GetOrdinal(fieldPath.FinalField.ProjectionAlias);
			return !_queryResult.IsDBNull(ord);
		}

		public IGraphReaderEnumerator<EntityModel, EntityField> GetEnumerator<T>(IFieldPath<EntityModel, EntityField> fieldPath)
		{
			throw new NotImplementedException();
		}

		public T Read<T>(IFieldPath<EntityModel, EntityField> fieldPath)
		{
			if (typeof(T).IsEnum)
				return (T)(object)Read<int>(fieldPath);

			var ord = _queryResult.GetOrdinal(fieldPath.FinalField.ProjectionAlias);

			if (_queryResult.IsDBNull(ord))
				return default(T);

			if (!_typeReaders.TryGetValue(typeof(T), out var typeReader))
				throw new InvalidOperationException($"Type `{typeof(T).FullName}` not supported.");
			var usefulTypeReader = typeReader as Func<QueryResult, int, T>;
			return usefulTypeReader(_queryResult, ord);
		}
	}
}
