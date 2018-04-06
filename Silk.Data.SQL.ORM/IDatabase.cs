using Silk.Data.SQL.Providers;
using System.Collections.Generic;
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
		/// Gets the schema that contains knowledge of the entity type.
		/// </summary>
		Schema.Schema DataSchema { get; }
		/// <summary>
		/// Gets the data provider used to execute queries.
		/// </summary>
		IDataProvider DataProvider { get; }

		void Insert(IEnumerable<T> entities);
		void Insert<TView>(IEnumerable<TView> entities) where TView : class;
		Task InsertAsync(IEnumerable<T> entities);
		Task InsertAsync<TView>(IEnumerable<TView> entities) where TView : class;
	}
}
