using Silk.Data.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
				{ typeof(uint), SqlDataType.UnsignedInt() },
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

		public static ConstructorInfo GetConstructor(Type type)
		{
			var ctor = type.GetConstructors()
				.FirstOrDefault(q => q.GetParameters().Length == 0);
			if (ctor == null)
			{
				throw new MappingRequirementException($"A constructor with 0 parameters is required on type {type}.");
			}
			return ctor;
		}
	}
}
