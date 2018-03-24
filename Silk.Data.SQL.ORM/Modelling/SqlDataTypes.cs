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
			var typeInfo = type.GetTypeInfo();
			if (typeInfo.IsEnum)
				return IsSQLPrimitiveType(typeof(int));
			if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
				return IsSQLPrimitiveType(typeInfo.GenericTypeArguments[0]);
			return _sqlDataTypes.ContainsKey(type);
		}

		public static Column CreateColumn(IField field, EntityFieldOptions options, string sqlColumnName,
			bool isPrimaryKey, bool isAutoIncrement, bool isAutoGenerated)
		{
			var typeInfo = field.FieldType.GetTypeInfo();
			var dataType = field.FieldType;
			if (typeInfo.IsEnum)
				dataType = typeof(int);

			var isNullable = dataType == typeof(string);
			if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				dataType = typeInfo.GenericTypeArguments[0];
				isNullable = true;
			}

			if (!_sqlDataTypes.TryGetValue(dataType, out var getSqlType))
				return null;
			return new Column(sqlColumnName, getSqlType(options),
				isPrimaryKey: isPrimaryKey, isAutoIncrement: isAutoIncrement,
				isAutoGenerated: isAutoGenerated, isNullable: isNullable);
		}
	}
}
