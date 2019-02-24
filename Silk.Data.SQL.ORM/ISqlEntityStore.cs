using Silk.Data.SQL.ORM.Queries;
using System;

namespace Silk.Data.SQL.ORM
{
	public interface ISqlEntityStore<T> : IEntityStore<T>
		where T : class
	{
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
		IDeferred Insert(Action<InsertBuilder<T>> queryConfigurer);

		IDeferred Delete(T entity);
		IDeferred Delete(IEntityReference<T> entityReference);
		IDeferred Delete(Action<DeleteBuilder<T>> queryConfigurer);

		//IDeferred Update(T entity);
		//IDeferred Update<TView>(IEntityReference<T> entityReference, TView view);
	}
}
