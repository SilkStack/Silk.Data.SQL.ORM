using System;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Schema
{
	internal static class SqlTypeHelper
	{
		private static readonly Dictionary<Type, SqlDataType> _sqlTypeDictionary
			= new Dictionary<Type, SqlDataType>()
			{
				{ typeof(string), SqlDataType.Text() },
				{ typeof(bool), SqlDataType.Bit() },
				{ typeof(sbyte), SqlDataType.TinyInt() },
				{ typeof(byte), SqlDataType.UnsignedTinyInt() },
				{ typeof(ushort), SqlDataType.UnsignedSmallInt() },
				{ typeof(short), SqlDataType.SmallInt() },
				{ typeof(uint), SqlDataType.UnsignedSmallInt() },
				{ typeof(int), SqlDataType.Int() },
				{ typeof(ulong), SqlDataType.UnsignedBigInt() },
				{ typeof(long), SqlDataType.BigInt() },
				{ typeof(float), SqlDataType.Float(SqlDataType.FLOAT_MAX_PRECISION) },
				{ typeof(double), SqlDataType.Float(SqlDataType.DOUBLE_MAX_PRECISION) },
				{ typeof(decimal), SqlDataType.Decimal() },
				{ typeof(DateTime), SqlDataType.DateTime() },
				{ typeof(Guid), SqlDataType.Guid() }
			};

		public static bool IsSqlPrimitiveType(Type type)
		{
			if (type.IsEnum)
				return IsSqlPrimitiveType(typeof(int));
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				return IsSqlPrimitiveType(type.GetGenericArguments()[0]);
			return _sqlTypeDictionary.ContainsKey(type);
		}

		public static SqlDataType GetDataType(Type type)
		{
			if (type.IsEnum)
				return GetDataType(typeof(int));
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				return GetDataType(type.GetGenericArguments()[0]);
			_sqlTypeDictionary.TryGetValue(type, out var sqlDataType);
			return sqlDataType;
		}
	}
}
