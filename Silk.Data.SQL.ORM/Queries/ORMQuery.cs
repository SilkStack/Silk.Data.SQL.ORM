using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Queries;
using System;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public abstract class ORMQuery
	{
		public Type MapToType { get; }
		public bool IsQueryResult { get; }
		public QueryExpression Query { get; }

		public ORMQuery(QueryExpression query, Type mapToType = null,
			bool isQueryResult = false)
		{
			Query = query;
			MapToType = mapToType;
			IsQueryResult = isQueryResult;
		}

		public virtual object MapResult(QueryResult queryResult)
		{
			return null;
		}

		public virtual Task<object> MapResultAsync(QueryResult queryResult)
		{
			return null;
		}
	}
}
