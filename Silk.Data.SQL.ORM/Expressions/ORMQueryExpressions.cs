using Silk.Data.SQL.Expressions;
using System;

namespace Silk.Data.SQL.ORM.Queries
{
	public static class ORMQueryExpressions
	{
		public static ValueExpression Value(object value)
		{
			if (value is Enum)
				value = (int)value;
			return QueryExpression.Value(value);
		}
	}
}
