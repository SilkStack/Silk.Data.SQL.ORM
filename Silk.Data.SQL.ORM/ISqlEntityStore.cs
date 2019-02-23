using Silk.Data.SQL.ORM.Queries;
using System;

namespace Silk.Data.SQL.ORM
{
	public interface ISqlEntityStore<T> : IEntityStore<T>
		where T : class
	{
		/// <summary>
		/// Insert a collection of entities.
		/// </summary>
		/// <param name="entities"></param>
		/// <returns></returns>
		IDeferred Insert(params T[] entities);

		/// <summary>
		/// Insert a collection of entities using a view type.
		/// </summary>
		/// <typeparam name="TView"></typeparam>
		/// <param name="entityViews"></param>
		/// <returns></returns>
		IDeferred Insert<TView>(params TView[] entityViews)
			where TView : class;

		/// <summary>
		/// Insert a custom entity record.
		/// </summary>
		/// <param name="queryConfigurer"></param>
		/// <returns></returns>
		IDeferred Insert(Action<InsertBuilder<T>> queryConfigurer);
	}
}
