﻿using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Modelling
{
	public static class SqlDataTypes
	{
		private readonly static Dictionary<Type, Func<EntityFieldOptions, SqlDataType>> _sqlDataTypes
			= new Dictionary<Type, Func<EntityFieldOptions, SqlDataType>>
			{
				{ typeof(string), options => options?.ConfiguredDataLength == null ? SqlDataType.Text() : SqlDataType.Text(options.ConfiguredDataLength.Value) },
				{ typeof(bool), options => SqlDataType.Bit() },
				{ typeof(byte), options => SqlDataType.UnsignedTinyInt() },
				{ typeof(sbyte), options => SqlDataType.TinyInt() },
				{ typeof(short), options => SqlDataType.SmallInt() },
				{ typeof(ushort), options => SqlDataType.UnsignedSmallInt() },
				{ typeof(int), options => SqlDataType.Int() },
				{ typeof(uint), options => SqlDataType.UnsignedInt() },
				{ typeof(long), options => SqlDataType.BigInt() },
				{ typeof(ulong), options => SqlDataType.UnsignedBigInt() },
				{ typeof(float), options => options?.ConfiguredPrecision == null ? SqlDataType.Float(SqlDataType.FLOAT_MAX_PRECISION) : SqlDataType.Float(options.ConfiguredPrecision.Value) },
				{ typeof(double), options => options?.ConfiguredPrecision == null ? SqlDataType.Float(SqlDataType.DOUBLE_MAX_PRECISION) : SqlDataType.Float(options.ConfiguredPrecision.Value) },
				{ typeof(decimal), options => {
					if (options?.ConfiguredPrecision != null && options?.ConfiguredScale != null)
						return SqlDataType.Decimal(options.ConfiguredPrecision.Value, options.ConfiguredScale.Value);
					else if (options?.ConfiguredPrecision != null)
						return SqlDataType.Decimal(options.ConfiguredPrecision.Value);
					return SqlDataType.Decimal();
					} },
				{ typeof(Guid), options => SqlDataType.Guid() },
				{ typeof(DateTime), options => SqlDataType.DateTime() }
			};

		public static bool IsSQLPrimitiveType(Type type)
		{
			if (type.GetTypeInfo().IsEnum)
				return IsSQLPrimitiveType(typeof(int));
			return _sqlDataTypes.ContainsKey(type);
		}

		public static SqlDataType GetSqlDataType(IField field, EntityFieldOptions options)
		{
			var dataType = field.FieldType;
			if (field.FieldType.GetTypeInfo().IsEnum)
				dataType = typeof(int);

			if (!_sqlDataTypes.TryGetValue(dataType, out var func))
				return null;
			return func(options);
		}
	}
}