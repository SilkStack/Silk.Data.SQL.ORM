using System;
using System.Collections.Generic;
using System.Text;

namespace Silk.Data.SQL.ORM.Queries
{
	public interface IQueryHelper<TEntity> where TEntity : class
	{
		IEntityQueryBuilder<TEntity> CreateQuery();
	}
}
