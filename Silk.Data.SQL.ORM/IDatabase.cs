﻿using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.Providers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM
{
	/// <summary>
	/// Database of <see cref="T"/>.
	/// </summary>
	/// <typeparam name="T">Entity type to store in the database.</typeparam>
	public interface IDatabase<T> where T : class
	{
		/// <summary>
		/// Gets the data provider used to execute queries.
		/// </summary>
		IDataProvider DataProvider { get; }

		/// <summary>
		/// Gets the model of the entity type stored in the database.
		/// </summary>
		EntityModel<T> EntityModel { get; }

		void Insert(IEnumerable<T> entities);
		void Insert<TView>(IEnumerable<TView> entities) where TView : class;
		Task InsertAsync(IEnumerable<T> entities);
		Task InsertAsync<TView>(IEnumerable<TView> entities) where TView : class;

		ICollection<T> Query(Condition where = null, Condition having = null, OrderBy orderBy = null,
			GroupBy groupBy = null, int? offset = null, int? limit = null);
		Task<ICollection<T>> QueryAsync(Condition where = null, Condition having = null, OrderBy orderBy = null,
			GroupBy groupBy = null, int? offset = null, int? limit = null);
		ICollection<TView> Query<TView>(Condition where = null, Condition having = null, OrderBy orderBy = null,
			GroupBy groupBy = null, int? offset = null, int? limit = null)
			where TView : class;
		Task<ICollection<TView>> QueryAsync<TView>(Condition where = null, Condition having = null, OrderBy orderBy = null,
			GroupBy groupBy = null, int? offset = null, int? limit = null)
			where TView : class;

		void Delete(IEnumerable<T> entities);
		Task DeleteAsync(IEnumerable<T> entities);
		void Delete(Condition where);
		Task DeleteAsync(Condition where);

		void Update(IEnumerable<T> entities);
		Task UpdateAsync(IEnumerable<T> entities);
		void Update<TView>(TView view, Condition where)
			where TView : class;
		Task UpdateAsync<TView>(TView view, Condition where)
			where TView : class;
	}
}
