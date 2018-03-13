using Silk.Data.SQL.ORM.Modelling;

namespace Silk.Data.SQL.ORM.Operations
{
	public class InsertOperation : DataOperation
	{
		public static InsertOperation Create<TEntity>(EntityModel<TEntity> model, params TEntity[] entities)
		{
			return null;
		}

		public static InsertOperation Create<TEntity, TProjection>(EntityModel<TEntity> model, params TProjection[] projections)
		{
			return null;
		}
	}
}
