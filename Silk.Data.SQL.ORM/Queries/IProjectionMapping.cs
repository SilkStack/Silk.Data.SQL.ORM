using Silk.Data.Modelling;
using Silk.Data.SQL.Queries;

namespace Silk.Data.SQL.ORM.Queries
{
	public interface IProjectionMapping<T>
	{
		IModelReadWriter CreateReader(QueryResult queryResult);
		void Inject(T obj, IModelReadWriter readWriter);
		T Map(IModelReadWriter readWriter);
	}
}
