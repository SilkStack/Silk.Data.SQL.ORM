namespace Silk.Data.SQL.ORM
{
	public static class DatabaseFunctions
	{
		public static bool Like(string str, string pattern) => false;
		public static int Count(object expr) => 0;
		public static T Alias<T>(T expr, string alias) => default(T);
		public static int Random() => 0;
	}
}
