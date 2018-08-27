using System.Threading.Tasks;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Providers;
using Silk.Data.SQL.Queries;
using Silk.Data.SQL.SQLite3;

namespace Silk.Data.SQL.ORM.Tests
{
	public static class TestHelper
	{
		public const string ConnectionString = "Data Source=:memory:;Mode=Memory";

		public static IDataProvider CreateProvider()
		{
			return new DataProviderDebuggingProxy(new SQLite3DataProvider(ConnectionString));
		}

		private class DataProviderDebuggingProxy : IDataProvider
		{
			private readonly DataProviderCommonBase _baseProvider;

			public DataProviderDebuggingProxy(DataProviderCommonBase baseProvider)
			{
				_baseProvider = baseProvider;
			}

			public string ProviderName => _baseProvider.ProviderName;

			public ITransaction CreateTransaction() => _baseProvider.CreateTransaction();

			public Task<ITransaction> CreateTransactionAsync() => _baseProvider.CreateTransactionAsync();

			public void Dispose() => _baseProvider.Dispose();

			public int ExecuteNonQuery(QueryExpression queryExpression) => _baseProvider.ExecuteNonQuery(queryExpression);

			public Task<int> ExecuteNonQueryAsync(QueryExpression queryExpression)
			{
				var query = _baseProvider.ConvertExpressionToQuery(queryExpression);
				return _baseProvider.ExecuteNonQueryAsync(queryExpression);
			}

			public QueryResult ExecuteReader(QueryExpression queryExpression) => _baseProvider.ExecuteReader(queryExpression);

			public Task<QueryResult> ExecuteReaderAsync(QueryExpression queryExpression) => _baseProvider.ExecuteReaderAsync(queryExpression);
		}
	}
}
