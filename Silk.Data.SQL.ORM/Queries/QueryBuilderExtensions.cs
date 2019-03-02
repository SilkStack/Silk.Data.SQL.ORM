using Silk.Data.SQL.Providers;

namespace Silk.Data.SQL.ORM.Queries
{
	public static class QueryBuilderExtensions
	{
		public static SingleDeferableSelect<TEntity, TView> CreateSingleResultQuery<TEntity, TView>(
			this IEntitySelectQueryBuilder<TEntity> queryBuilder,
			IResultReader<TView> resultReader,
			IDataProvider dataProvider
			)
			where TEntity : class
		{
			return new SingleDeferableSelect<TEntity, TView>(
				queryBuilder, dataProvider, resultReader
				);
		}

		public static MultipleDeferableSelect<TEntity, TView> CreateMultipleResultQuery<TEntity, TView>(
			this IEntitySelectQueryBuilder<TEntity> queryBuilder,
			IResultReader<TView> resultReader,
			IDataProvider dataProvider
			)
			where TEntity : class
		{
			return new MultipleDeferableSelect<TEntity, TView>(
				queryBuilder, dataProvider, resultReader
				);
		}
	}
}
