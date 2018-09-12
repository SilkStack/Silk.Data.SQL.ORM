using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM
{
	public static class QueryResultExtensions
	{
		private static readonly Dictionary<SqlBaseType, Func<QueryResult, int, object>> _typeReaders =
			new Dictionary<SqlBaseType, Func<QueryResult, int, object>>()
			{
				{ SqlBaseType.Bit, (q,o) => q.GetBoolean(o) },
				{ SqlBaseType.TinyInt, (q,o) => q.GetByte(o) },
				{ SqlBaseType.SmallInt, (q,o) => q.GetInt16(o) },
				{ SqlBaseType.Int, (q,o) => q.GetInt32(o) },
				{ SqlBaseType.BigInt, (q,o) => q.GetInt64(o) },
				{ SqlBaseType.Float, (q,o) => q.GetDouble(o) },
				{ SqlBaseType.Decimal, (q,o) => q.GetDecimal(o) },
				{ SqlBaseType.Text, (q,o) => q.GetString(o) },
				{ SqlBaseType.Guid, (q,o) => q.GetGuid(o) },
				{ SqlBaseType.DateTime, (q,o) => q.GetDateTime(o) }
			};

		public static object GetColumnValue(this QueryResult queryResult, Column column)
		{
			if (!_typeReaders.TryGetValue(column.DataType.BaseType, out var reader))
				throw new Exception("Unsupported data type.");

			if (column.DataType.BaseType == SqlBaseType.Float &&
				column.DataType.Parameters[0] <= SqlDataType.FLOAT_MAX_PRECISION)
				reader = (q, o) => q.GetFloat(o);

			var ord = queryResult.GetOrdinal(column.ColumnName);
			if (queryResult.IsDBNull(ord))
				return null;

			return reader(queryResult, ord);
		}
	}
}
