using System;
using System.Collections.Generic;
using System.Text;

namespace Silk.Data.SQL.ORM.Schema
{
	internal static class SqlTypeHelper
	{
		private static readonly Dictionary<Type, object> _sqlTypeDictionary
			= new Dictionary<Type, object>()
			{
				{ typeof(string), null },
				{ typeof(bool), null },
				{ typeof(sbyte), null },
				{ typeof(byte), null },
				{ typeof(ushort), null },
				{ typeof(short), null },
				{ typeof(uint), null },
				{ typeof(int), null },
				{ typeof(ulong), null },
				{ typeof(long), null },
				{ typeof(float), null },
				{ typeof(double), null },
				{ typeof(decimal), null },
				{ typeof(DateTime), null },
				{ typeof(Guid), null }
			};

		public static bool IsSqlPrimitiveType(Type type)
		{
			if (type.IsEnum)
				return IsSqlPrimitiveType(typeof(int));
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				return IsSqlPrimitiveType(type.GetGenericArguments()[0]);
			return _sqlTypeDictionary.ContainsKey(type);
		}
	}
}
