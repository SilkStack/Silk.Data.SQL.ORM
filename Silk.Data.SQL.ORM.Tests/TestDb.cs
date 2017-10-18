using Silk.Data.SQL.SQLite3;

namespace Silk.Data.SQL.ORM.Tests
{
	public static class TestDb
	{
		public static SQLite3DataProvider Provider { get; } =
			new SQLite3DataProvider(":memory:");
	}
}
