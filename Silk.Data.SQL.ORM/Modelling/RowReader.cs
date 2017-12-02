using Silk.Data.Modelling;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class RowReader
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

		private readonly ViewReadWriter _viewReadWriter;

		public RowReader(ViewReadWriter viewReadWriter)
		{
			_viewReadWriter = viewReadWriter;
		}

		public void ReadRow(QueryResult queryResult)
		{
			foreach (var field in _viewReadWriter.View.Fields
				.OfType<DataField>()
				.Where(q => q.Storage.Table.IsEntityTable &&
				q.Relationship == null))
			{
				if (!_typeReaders.TryGetValue(field.DataType, out var readFunc))
					throw new InvalidOperationException("Unsupported data type.");

				var ord = queryResult.GetOrdinal(field.Name);

				if (queryResult.IsDBNull(ord))
					_viewReadWriter.WriteToPath<object>(new[] { field.Name }, null);
				else
					_viewReadWriter.WriteToPath<object>(new[] { field.Name }, readFunc(queryResult, ord));
			}
		}
	}
}
