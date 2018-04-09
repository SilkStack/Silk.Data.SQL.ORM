using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class ConditionConverter<T>
	{
		public QueryExpression ConvertToCondition(Expression<Func<T, bool>> expression, EntityModel<T> entityModel)
		{
			return null;
		}
	}
}
