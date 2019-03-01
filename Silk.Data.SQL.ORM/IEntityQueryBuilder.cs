using Silk.Data.SQL.ORM.Queries;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM
{
	public interface IEntityQueryBuilder<T>
		where T : class
	{
		/// <summary>
		/// Prepare an insert entity query.
		/// </summary>
		/// <param name="entities"></param>
		/// <returns></returns>
		IEntityInsertQueryBuilder<T> Insert(T entity);

		/// <summary>
		/// Prepare an insert view query.
		/// </summary>
		/// <typeparam name="TView"></typeparam>
		/// <param name="entityViews"></param>
		/// <returns></returns>
		IEntityInsertQueryBuilder<T> Insert<TView>(TView entityView)
			where TView : class;

		/// <summary>
		/// Parent to insert a custom entity record.
		/// </summary>
		/// <param name="queryConfigurer"></param>
		/// <returns></returns>
		IEntityInsertQueryBuilder<T> Insert(Action<IEntityInsertQueryBuilder<T>> queryConfigurer);

		IEntityDeleteQueryBuilder<T> Delete(T entity);
		IEntityDeleteQueryBuilder<T> Delete(IEntityReference<T> entityReference);
		IEntityDeleteQueryBuilder<T> Delete(Action<IEntityDeleteQueryBuilder<T>> queryConfigurer);

		IEntityUpdateQueryBuilder<T> Update(T entity);
		IEntityUpdateQueryBuilder<T> Update<TView>(IEntityReference<T> entityReference, TView view)
			where TView : class;
		IEntityUpdateQueryBuilder<T> Update(Action<IEntityUpdateQueryBuilder<T>> queryConfigurer);

		IEntitySelectQueryBuilder<T> Select(IEntityReference<T> entityReference);
		IEntitySelectQueryBuilder<T> Select(IEntityReference<T> entityReference, out IResultReader<T> resultReader);

		IEntitySelectQueryBuilder<T> Select<TView>(IEntityReference<T> entityReference)
			where TView : class;
		IEntitySelectQueryBuilder<T> Select<TView>(IEntityReference<T> entityReference, out IResultReader<TView> resultReader)
			where TView : class;

		IEntitySelectQueryBuilder<T> Select(Action<IEntitySelectQueryBuilder<T>> query);
		IEntitySelectQueryBuilder<T> Select(Action<IEntitySelectQueryBuilder<T>> query, out IResultReader<T> resultReader);

		IEntitySelectQueryBuilder<T> Select<TView>(Action<IEntitySelectQueryBuilder<T>> query)
			where TView : class;
		IEntitySelectQueryBuilder<T> Select<TView>(Action<IEntitySelectQueryBuilder<T>> query, out IResultReader<TView> resultReader)
			where TView : class;

		IEntitySelectQueryBuilder<T> Select<TExpr>(Expression<Func<T, TExpr>> expression, Action<IEntitySelectQueryBuilder<T>> query);
		IEntitySelectQueryBuilder<T> Select<TExpr>(Expression<Func<T, TExpr>> expression, Action<IEntitySelectQueryBuilder<T>> query, out IResultReader<TExpr> resultReader);
	}
}
