﻿using Silk.Data.SQL.ORM.Operations;
using Silk.Data.SQL.Providers;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM
{
	public static class ExecutionExtensions
	{
		public static void ExecuteBulk(this IDataProvider dataProvider, BulkOperation bulkOperation)
		{
			foreach (var operation in bulkOperation.GetOperations())
			{
				using (var queryResult = dataProvider.ExecuteReader(operation.GetQuery()))
				{
					operation.ProcessResult(queryResult);
				}
			}
		}

		public static async Task ExecuteBulkAsync(this IDataProvider dataProvider, BulkOperation bulkOperation)
		{
			foreach (var operation in bulkOperation.GetOperations())
			{
				using (var queryResult = await dataProvider.ExecuteReaderAsync(operation.GetQuery()))
				{
					await operation.ProcessResultAsync(queryResult);
				}
			}
		}

		public static void ExecuteNonReader(this IDataProvider dataProvider, DataOperation operation)
		{
			var query = operation.GetQuery();
			if (query != null)
				dataProvider.ExecuteNonQuery(query);
		}

		public static Task ExecuteNonReaderAsync(this IDataProvider dataProvider, DataOperation operation)
		{
			var query = operation.GetQuery();
			if (query != null)
				return dataProvider.ExecuteNonQueryAsync(operation.GetQuery());
			return Task.CompletedTask;
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

		public static void Insert(this IDataProvider dataProvider, InsertOperation insert)
		{
			if (!insert.GeneratesValuesServerSide)
			{
				dataProvider.ExecuteNonReader(insert);
				return;
			}

			using (var queryResult = dataProvider.ExecuteReader(insert.GetQuery()))
				insert.ProcessResult(queryResult);
		}

		public static async Task InsertAsync(this IDataProvider dataProvider, InsertOperation insert)
		{
			if (!insert.GeneratesValuesServerSide)
			{
				await dataProvider.ExecuteNonReaderAsync(insert);
				return;
			}

			using (var queryResult = await dataProvider.ExecuteReaderAsync(insert.GetQuery()))
				await insert.ProcessResultAsync(queryResult);
		}
	}
}
