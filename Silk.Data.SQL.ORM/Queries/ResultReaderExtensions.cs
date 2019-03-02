namespace Silk.Data.SQL.ORM.Queries
{
	public static class ResultReaderExtensions
	{
		public static IResultReader<(T1, T2)> Combine<T1, T2>(this IResultReader<T1> resultReader1, IResultReader<T2> resultReader2)
		{
			return new CompositeResultReader<T1, T2>(resultReader1, resultReader2);
		}
	}
}
