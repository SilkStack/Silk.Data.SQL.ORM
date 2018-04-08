using Silk.Data.SQL.ORM.Operations;
using Silk.Data.SQL.Providers;
using Silk.Data.SQL.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM
{
	public static class ExecutionExtensions
	{
		public static void ExecuteNonReader(this IDataProvider dataProvider, DataOperation operation)
		{
			dataProvider.ExecuteNonQuery(operation.GetQuery());
		}

		public static Task ExecuteNonReaderAsync(this IDataProvider dataProvider, DataOperation operation)
		{
			return dataProvider.ExecuteNonQueryAsync(operation.GetQuery());
		}

		public static void ExecuteReader<T>(this IDataProvider dataProvider, DataOperationWithResult<T> operation)
		{
			using (var result = dataProvider.ExecuteReader(operation.GetQuery()))
				operation.ProcessResult(result);
		}

		public static async Task ExecuteReaderAsync<T>(this IDataProvider dataProvider, DataOperationWithResult<T> operation)
		{
			using (var result = await dataProvider.ExecuteReaderAsync(operation.GetQuery()))
				await operation.ProcessResultAsync(result);
		}

		public static T Get<T>(this IDataProvider dataProvider, DataOperationWithResult<T> operation)
		{
			dataProvider.ExecuteReader(operation);
			return operation.Result;
		}

		public static async Task<T> GetAsync<T>(this IDataProvider dataProvider, DataOperationWithResult<T> operation)
		{
			await dataProvider.ExecuteReaderAsync(operation);
			return operation.Result;
		}

		public static T GetSingle<T>(this IDataProvider dataProvider, SelectOperation<T> select)
		{
			return dataProvider.Get(select).FirstOrDefault();
		}

		public static async Task<T> GetSingleAsync<T>(this IDataProvider dataProvider, SelectOperation<T> select)
		{
			return (await dataProvider.GetAsync(select)).FirstOrDefault();
		}
	}
}
