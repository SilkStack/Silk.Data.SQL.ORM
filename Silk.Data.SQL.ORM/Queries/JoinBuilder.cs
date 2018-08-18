using System;
using System.Collections.Generic;
using System.Text;
using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public class JoinBuilder : IExpressionBuilder
	{
		public QueryExpression BuildExpression()
		{
			throw new NotImplementedException();
		}
	}

	public class JoinBuilder<TEntity, TRelated> : JoinBuilder
		where TEntity : class
		where TRelated : class
	{
	}
}
