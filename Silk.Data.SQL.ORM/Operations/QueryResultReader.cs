using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;
using System.Linq;

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

		private IField GetField(string[] path, int startOffset)
		{
			IField ret = null;
			var fields = Model.Fields;
			for (var i = startOffset; i < path.Length; i++)
			{
				ret = fields.FirstOrDefault(q => q.FieldName == path[i]);
				if (ret == null)
					break;
				if (ret is ISingleRelatedObjectField singleRelatedObjectField)
					fields = singleRelatedObjectField.RelatedObjectModel.Fields;
				else if (ret is IEmbeddedObjectField embeddedObjectField)
					fields = embeddedObjectField.EmbeddedFields;
			}
			return ret;
		}

		private Type GetDataType(IField field)
		{
			if (field is IValueField valueField)
				return field.FieldType;
			else if (field is ISingleRelatedObjectField singleRelatedObjectField)
				return singleRelatedObjectField.RelatedPrimaryKey.FieldType;
			else if (field is IEmbeddedObjectField embeddedObjectField)
				return typeof(bool); // null check for the embedded object
			else if (field is IProjectionField projectionField)
				return GetDataType(projectionField.FieldPath.Last());
			return null;
		}

		public T ReadField<T>(string[] path, int offset)
		{
			var field = GetField(path, offset);
			if (field == null)
				throw new Exception("Unknown field on model.");

			var dataType = GetDataType(field);

			if (dataType == null)
				return default(T);

			if (!_typeReaders.TryGetValue(dataType, out var readFunc))
				return default(T);

			var fieldAlias = string.Join("_", path);
			var ord = _queryResult.GetOrdinal(fieldAlias);
			if (_queryResult.IsDBNull(ord))
				return default(T);
			return (T)readFunc(_queryResult, ord);
		}

		public void WriteField<T>(string[] path, int offset, T value)
		{
			throw new System.NotImplementedException();
		}
	}
}
