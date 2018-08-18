using System;
using System.Collections.Generic;
using System.Text;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Queries
{
	public class QueryHelper<TEntity> : IQueryHelper<TEntity>
		where TEntity : class
	{
		public Schema.Schema Schema { get; }
		public EntityModel<TEntity> EntityModel { get; }

		public QueryHelper(Schema.Schema schema)
		{
			Schema = schema;
			EntityModel = schema.GetEntityModel<TEntity>();
		}

		public IEntityQueryBuilder<TEntity> CreateQuery()
		{
			return new EntityQueryBuilder<TEntity>(Schema);
		}
	}
}
