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
		/// <returns></returns>
		IEntityInsertQueryBuilder<T> Insert();

		IEntityDeleteQueryBuilder<T> Delete(T entity);
		IEntityDeleteQueryBuilder<T> Delete(IEntityReference<T> entityReference);
		IEntityDeleteQueryBuilder<T> Delete();

		IEntityUpdateQueryBuilder<T> Update(T entity);
		IEntityUpdateQueryBuilder<T> Update<TView>(IEntityReference<T> entityReference, TView view)
			where TView : class;
		IEntityUpdateQueryBuilder<T> Update();

		IEntitySelectQueryBuilder<T> Select(IEntityReference<T> entityReference);
		IEntitySelectQueryBuilder<T> Select(IEntityReference<T> entityReference, out IResultReader<T> resultReader);

		IEntitySelectQueryBuilder<T> Select<TView>(IEntityReference<T> entityReference)
			where TView : class;
		IEntitySelectQueryBuilder<T> Select<TView>(IEntityReference<T> entityReference, out IResultReader<TView> resultReader)
			where TView : class;

		IEntitySelectQueryBuilder<T> Select();
		IEntitySelectQueryBuilder<T> Select(out IResultReader<T> resultReader);

		IEntitySelectQueryBuilder<T> Select<TView>()
			where TView : class;
		IEntitySelectQueryBuilder<T> Select<TView>(out IResultReader<TView> resultReader)
			where TView : class;

		IEntitySelectQueryBuilder<T> Select<TExpr>(Expression<Func<T, TExpr>> expression);
		IEntitySelectQueryBuilder<T> Select<TExpr>(Expression<Func<T, TExpr>> expression, out IResultReader<TExpr> resultReader);
	}
}
