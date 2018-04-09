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

		void Insert(IEnumerable<T> entities);
		void Insert<TView>(IEnumerable<TView> entities) where TView : class;
		Task InsertAsync(IEnumerable<T> entities);
		Task InsertAsync<TView>(IEnumerable<TView> entities) where TView : class;

		ICollection<T> Query(Condition where = null, Condition having = null, OrderBy orderBy = null,
			GroupBy groupBy = null, int? offset = null, int? limit = null);
		Task<ICollection<T>> QueryAsync(Condition where = null, Condition having = null, OrderBy orderBy = null,
			GroupBy groupBy = null, int? offset = null, int? limit = null);
	}
}
