using Silk.Data.SQL.ORM.Queries;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM
{
	public interface ISqlEntityStore<T> : IEntityStore<T>
		where T : class
	{
		/// <summary>
		/// Gets or sets the query builder being used to construct queries.
		/// </summary>
		IEntityQueryBuilder<T> QueryBuilder { get; set; }

		/// <summary>
		/// Insert an entity.
		/// </summary>
		/// <param name="entities"></param>
		/// <returns></returns>
		IDeferred Insert(T entity);

		/// <summary>
		/// Insert an entities using a view type.
		/// </summary>
		/// <typeparam name="TView"></typeparam>
		/// <param name="entityViews"></param>
		/// <returns></returns>
		IDeferred Insert<TView>(TView entityView)
			where TView : class;

		/// <summary>
		/// Insert a custom entity record.
		/// </summary>
		/// <param name="queryConfigurer"></param>
		/// <returns></returns>
		IDeferred Insert(Action<IEntityInsertQueryBuilder<T>> queryConfigurer);

		IDeferred Delete(T entity);
		IDeferred Delete(IEntityReference<T> entityReference);
		IDeferred Delete(Action<IEntityDeleteQueryBuilder<T>> queryConfigurer);

		IDeferred Update(T entity);
		IDeferred Update<TView>(IEntityReference<T> entityReference, TView view)
			where TView : class;
		IDeferred Update(Action<IEntityUpdateQueryBuilder<T>> queryConfigurer);

		/// <summary>
		/// Selects a single entity.
		/// </summary>
		/// <param name="entityReference"></param>
		/// <param name="entityResult"></param>
		/// <returns></returns>
		IDeferred Select(IEntityReference<T> entityReference, out DeferredResult<T> entityResult);
		/// <summary>
		/// Selects a single entity.
		/// </summary>
		/// <typeparam name="TView"></typeparam>
		/// <param name="entityReference"></param>
		/// <param name="viewResult"></param>
		/// <returns></returns>
		IDeferred Select<TView>(IEntityReference<T> entityReference, out DeferredResult<TView> viewResult)
			where TView : class;

		/// <summary>
		/// Selects multiple entities.
		/// </summary>
		/// <param name="query"></param>
		/// <param name="entitiesResult"></param>
		/// <returns></returns>
		IDeferred Select(Action<IEntitySelectQueryBuilder<T>> query, out DeferredResult<List<T>> entitiesResult);

		/// <summary>
		/// Selects multiple entities.
		/// </summary>
		/// <typeparam name="TView"></typeparam>
		/// <param name="query"></param>
		/// <param name="viewsResult"></param>
		/// <returns></returns>
		IDeferred Select<TView>(Action<IEntitySelectQueryBuilder<T>> query, out DeferredResult<List<TView>> viewsResult)
			where TView : class;

		/// <summary>
		/// Selects a custom expression.
		/// </summary>
		/// <typeparam name="TExpr"></typeparam>
		/// <param name="expression"></param>
		/// <param name="query"></param>
		/// <param name="exprsResult"></param>
		/// <returns></returns>
		IDeferred Select<TExpr>(Expression<Func<T, TExpr>> expression, Action<IEntitySelectQueryBuilder<T>> query,
			out DeferredResult<List<TExpr>> exprsResult);
	}
}
