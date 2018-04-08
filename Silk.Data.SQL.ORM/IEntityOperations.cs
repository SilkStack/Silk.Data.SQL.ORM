using Silk.Data.SQL.ORM.Operations;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM
{
	/// <summary>
	/// Common API for building operations for instances of <see cref="T"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IEntityOperations<T> where T : class
	{
		InsertOperation CreateInsert(IEnumerable<T> entities);
		InsertOperation CreateInsert(params T[] entities);
		InsertOperation CreateInsert<TView>(IEnumerable<TView> entities)
			where TView : class;
		InsertOperation CreateInsert<TView>(params TView[] entities)
			where TView : class;

		SelectOperation<T> CreateSelect(Expression<Func<T, bool>> where = null);
	}
}
