namespace Silk.Data.SQL.ORM
{
	public static class DatabaseFunctions
	{
		public static bool Like(string str, string pattern) => false;
		public static int Count(object expr) => 0;
		public static T Alias<T>(T expr, string alias) => default(T);
		public static int Random() => 0;
		public static bool IsIn(object checkFor, object searchExpr) => false;

		public static object Min(object expr) => null;
		public static byte Min(byte expr) => 0;
		public static short Min(short expr) => 0;
		public static int Min(int expr) => 0;
		public static long Min(long expr) => 0;
		public static float Min(float expr) => 0f;
		public static double Min(double expr) => 0d;
		public static decimal Min(decimal expr) => 0m;

		public static object Max(object expr) => null;
		public static byte Max(byte expr) => 0;
		public static short Max(short expr) => 0;
		public static int Max(int expr) => 0;
		public static long Max(long expr) => 0;
		public static float Max(float expr) => 0f;
		public static double Max(double expr) => 0d;
		public static decimal Max(decimal expr) => 0m;
	}
}
