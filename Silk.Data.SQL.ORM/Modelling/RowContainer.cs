using Silk.Data.Modelling;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class RowContainer : IContainer
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

		public TypedModel Model { get; }
		public IView View => _view;

		private DataModel _view;
		private Dictionary<string, object> _row = new Dictionary<string, object>();

		public RowContainer(TypedModel model, DataModel view)
		{
			Model = model;
			_view = view;
		}

		public void ReadRow(QueryResult queryResult)
		{
			foreach (var field in _view.Fields)
			{
				if (!_typeReaders.TryGetValue(field.DataType, out var readFunc))
					throw new InvalidOperationException("Unsupported data type.");

				var ord = queryResult.GetOrdinal(field.Storage.ColumnName);

				_row[field.Name] = readFunc(queryResult, ord);
			}
		}

		public object GetValue(string[] fieldPath)
		{
			if (fieldPath.Length != 1)
				throw new ArgumentOutOfRangeException(nameof(fieldPath), "Field path must have a length of 1.");
			return _row[fieldPath[0]];
		}

		public void SetValue(string[] fieldPath, object value)
		{
			throw new System.NotImplementedException();
		}
	}
}
