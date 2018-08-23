using Silk.Data.SQL.Providers;
using Silk.Data.SQL.SQLite3;

namespace Silk.Data.SQL.ORM.Tests
{
	public static class TestHelper
	{
		public const string ConnectionString = "Data Source=:memory:;Mode=Memory";

		public static IDataProvider CreateProvider()
		{
			return new SQLite3DataProvider(ConnectionString);
		}
	}
}
