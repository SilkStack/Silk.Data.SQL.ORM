using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Queries;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Operations
{
	public abstract class DataOperation
	{
		/// <summary>
		/// Gets a value indicating if the operation can be executed as part of a batch operation.
		/// </summary>
		public abstract bool CanBeBatched { get; }

		/// <summary>
		/// Gets the SQL query needed to run the DataOperation.
		/// </summary>
		/// <returns></returns>
		public abstract QueryExpression GetQuery();

		/// <summary>
		/// Process the result QueryResult set to the correct result set.
		/// </summary>
		public abstract void ProcessResult(QueryResult queryResult);

		/// <summary>
		/// Process the result QueryResult set to the correct result set.
		/// </summary>
		public abstract Task ProcessResultAsync(QueryResult queryResult);
	}

	public abstract class DataOperationWithResult<T> : DataOperation
	{
		/// <summary>
		/// Gets the result of the data operation.
		/// </summary>
		public abstract T Result { get; }
	}
}
