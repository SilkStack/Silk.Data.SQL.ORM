using Silk.Data.SQL.Expressions;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public interface IConditionalQuery
	{
		void AndWhere(QueryExpression queryExpression);
		void OrWhere(QueryExpression queryExpression);
	}

	public interface IEntityConditionalQuery<T> : IConditionalQuery
		where T : class
	{
		void AndWhere(Expression<Func<T, bool>> expression);
		void OrWhere(Expression<Func<T, bool>> expression);
	}
}
