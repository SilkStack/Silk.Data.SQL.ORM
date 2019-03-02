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
		DeferableInsert<T> Insert(T entity);

		/// <summary>
		/// Insert an entities using a view type.
		/// </summary>
		/// <typeparam name="TView"></typeparam>
		/// <param name="entityViews"></param>
		/// <returns></returns>
		DeferableInsert<T> Insert<TView>(TView entityView)
			where TView : class;

		/// <summary>
		/// Insert a custom entity record.
		/// </summary>
		/// <param name="queryConfigurer"></param>
		/// <returns></returns>
		DeferableInsert<T> Insert();

		DeferableDelete<T> Delete(T entity);
		DeferableDelete<T> Delete(IEntityReference<T> entityReference);
		DeferableDelete<T> Delete();

		DeferableUpdate<T> Update(T entity);
		DeferableUpdate<T> Update<TView>(IEntityReference<T> entityReference, TView view)
			where TView : class;
		DeferableUpdate<T> Update();

		/// <summary>
		/// Selects a single entity.
		/// </summary>
		/// <param name="entityReference"></param>
		/// <param name="entityResult"></param>
		/// <returns></returns>
		SingleDeferableSelect<T, T> Select(IEntityReference<T> entityReference);
		/// <summary>
		/// Selects a single entity.
		/// </summary>
		/// <typeparam name="TView"></typeparam>
		/// <param name="entityReference"></param>
		/// <param name="viewResult"></param>
		/// <returns></returns>
		SingleDeferableSelect<T, TView> Select<TView>(IEntityReference<T> entityReference)
			where TView : class;

		/// <summary>
		/// Selects multiple entities.
		/// </summary>
		/// <param name="query"></param>
		/// <param name="entitiesResult"></param>
		/// <returns></returns>
		MultipleDeferableSelect<T, T> Select();

		/// <summary>
		/// Selects multiple entities.
		/// </summary>
		/// <typeparam name="TView"></typeparam>
		/// <param name="query"></param>
		/// <param name="viewsResult"></param>
		/// <returns></returns>
		MultipleDeferableSelect<T, TView> Select<TView>()
			where TView : class;

		/// <summary>
		/// Selects a custom expression.
		/// </summary>
		/// <typeparam name="TExpr"></typeparam>
		/// <param name="expression"></param>
		/// <param name="query"></param>
		/// <param name="exprsResult"></param>
		/// <returns></returns>
		MultipleDeferableSelect<T, TExpr> Select<TExpr>(Expression<Func<T, TExpr>> expression);
	}
}
