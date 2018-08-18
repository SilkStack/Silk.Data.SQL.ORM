using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping.Binding;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Silk.Data.SQL.ORM.Queries
{
	public class BindingProjection<T> : IProjectionMapping<T>
	{
		private static readonly string[] _self = new[] { "." };

		private readonly ICollection<Binding> _bindings;
		private readonly TypeModel<T> _typeModel;

		public BindingProjection(ICollection<Binding> bindings)
		{
			_bindings = bindings;
			_typeModel = TypeModel.GetModelOf<T>();
		}

		public IModelReadWriter CreateReader(QueryResult queryResult)
		{
			return new ReadWriter(queryResult);
		}

		public void Inject(T obj, IModelReadWriter readWriter)
		{
			var writer = new ObjectReadWriter(obj, _typeModel, typeof(T));
			foreach (var binding in _bindings)
			{
				binding.PerformBinding(readWriter, writer);
			}
		}

		public T Map(IModelReadWriter readWriter)
		{
			var writer = new ObjectReadWriter(null, _typeModel, typeof(T));
			foreach (var binding in _bindings)
			{
				binding.PerformBinding(readWriter, writer);
			}
			return writer.ReadField<T>(_self, 0);
		}

		private class ReadWriter : IModelReadWriter
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
					{ typeof(bool?), (q,o) => q.GetBoolean(o) },
					{ typeof(byte?), (q,o) => q.GetByte(o) },
					{ typeof(short?), (q,o) => q.GetInt16(o) },
					{ typeof(int?), (q,o) => q.GetInt32(o) },
					{ typeof(long?), (q,o) => q.GetInt64(o) },
					{ typeof(float?), (q,o) => q.GetFloat(o) },
					{ typeof(double?), (q,o) => q.GetDouble(o) },
					{ typeof(decimal?), (q,o) => q.GetDecimal(o) },
					{ typeof(Guid?), (q,o) => q.GetGuid(o) },
					{ typeof(DateTime?), (q,o) => q.GetDateTime(o) },
				};

			private readonly QueryResult _queryResult;

			public IModel Model => throw new NotImplementedException();

			public ReadWriter(QueryResult queryResult)
			{
				_queryResult = queryResult;
			}

			public T1 ReadField<T1>(string[] path, int offset)
			{
				var fieldName = path[0];
				if (!_typeReaders.TryGetValue(typeof(T1), out var readerFunc))
					throw new Exception("Can't read type from SQL result.");
				var ord = _queryResult.GetOrdinal(fieldName);
				if (_queryResult.IsDBNull(ord))
					return default(T1);
				return (T1)readerFunc(_queryResult, ord);
			}

			public void WriteField<T1>(string[] path, int offset, T1 value)
			{
				throw new NotImplementedException();
			}
		}
	}
}
