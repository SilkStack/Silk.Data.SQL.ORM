using Silk.Data.SQL.ORM.Modelling;
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
		EntityModel<T> EntityModel { get; }

		InsertOperation CreateInsert(IEnumerable<T> entities);
		InsertOperation CreateInsert(params T[] entities);
		InsertOperation CreateInsert<TView>(IEnumerable<TView> entities)
			where TView : class;
		InsertOperation CreateInsert<TView>(params TView[] entities)
			where TView : class;

		SelectOperation<T> CreateSelect(Condition where = null, Condition having = null, OrderBy orderBy = null,
			GroupBy groupBy = null, int? offset = null, int? limit = null);
		SelectOperation<TView> CreateSelect<TView>(Condition where = null, Condition having = null, OrderBy orderBy = null,
			GroupBy groupBy = null, int? offset = null, int? limit = null)
			where TView : class;
		SelectOperation<int> CreateCount(Condition where = null, Condition having = null, GroupBy groupBy = null);

		DeleteOperation CreateDelete(IEnumerable<T> entities);
		DeleteOperation CreateDelete(params T[] entities);
		DeleteOperation CreateDelete(Condition where);

		UpdateOperation CreateUpdate(IEnumerable<T> entities);
		UpdateOperation CreateUpdate(params T[] entities);
		UpdateOperation CreateUpdate<TView>(TView view, Condition condition)
			where TView : class;
	}
}
