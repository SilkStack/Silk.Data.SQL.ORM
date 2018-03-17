using System.Threading.Tasks;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.Queries;

namespace Silk.Data.SQL.ORM.Operations
{
	public class InsertOperation : DataOperation
	{
		public override bool CanBeBatched => throw new System.NotImplementedException();

		public static InsertOperation Create<TEntity>(EntityModel<TEntity> model, params TEntity[] entities)
		{
			return null;
		}

		public static InsertOperation Create<TEntity, TProjection>(EntityModel<TEntity> model, params TProjection[] projections)
		{
			return null;
		}

		public override QueryExpression GetQuery()
		{
			throw new System.NotImplementedException();
		}

		public override void ProcessResult(QueryResult queryResult)
		{
			throw new System.NotImplementedException();
		}

		public override Task ProcessResultAsync(QueryResult queryResult)
		{
			throw new System.NotImplementedException();
		}
	}
}
