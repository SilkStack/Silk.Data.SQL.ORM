using Silk.Data.SQL.Queries;

namespace Silk.Data.SQL.ORM.Queries
{
	public class CompositeResultReader<T1, T2> : IResultReader<(T1, T2)>
	{
		private readonly IResultReader<T1> _resultReader1;
		private readonly IResultReader<T2> _resultReader2;

		public CompositeResultReader(IResultReader<T1> resultReader1, IResultReader<T2> resultReader2)
		{
			_resultReader1 = resultReader1;
			_resultReader2 = resultReader2;
		}

		public (T1, T2) Read(QueryResult queryResult)
		{
			return (
				_resultReader1.Read(queryResult),
				_resultReader2.Read(queryResult)
				);
		}
	}
}
