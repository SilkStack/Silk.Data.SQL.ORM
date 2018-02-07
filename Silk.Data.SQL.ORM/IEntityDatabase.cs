using Silk.Data.SQL.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM
{
	/// <summary>
	/// Common interface for performing operations on a single entity type.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TThis"></typeparam>
	public interface IEntityDatabaseBase<TEntity, TThis>
		where TThis : IEntityDatabaseBase<TEntity, TThis>
		where TEntity : new()
	{
		TThis Insert(params TEntity[] sources);
		TThis Insert<TProjection>(params TProjection[] sources)
			where TProjection : new();

		TThis Update(params TEntity[] sources);
		TThis Update<TProjection>(params TProjection[] sources)
			where TProjection : new();

		TThis Delete(params TEntity[] sources);
		TThis Delete<TProjection>(params TProjection[] sources)
			where TProjection : new();
		TThis Delete(QueryExpression where);

		TThis AsTransaction();
	}

	/// <summary>
	/// Performs common operations on a single entity type.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface IEntityDatabase<TEntity> : IEntityDatabaseBase<TEntity, IEntityDatabase<TEntity>>
		where TEntity : new()
	{
		IEntityDatabase<TEntity, TEntity> Select(QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null);

		IEntityDatabase<TEntity, TProjection> Select<TProjection>(QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null)
			where TProjection : new();

		IEntityDatabase<TEntity, int> SelectCount(QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] groupBy = null);

		IEntityDatabase<TEntity, int> SelectCount<TProjection>(QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] groupBy = null)
			where TProjection : new();

		void Execute();
		Task ExecuteAsync();
	}

	public interface IEntityDatabase<TEntity, TResult> : IEntityDatabaseBase<TEntity, IEntityDatabase<TEntity, TResult>>
		where TEntity : new()
	{
		ICollection<TResult> Execute();
		Task<ICollection<TResult>> ExecuteAsync();
	}
}
