using Silk.Data.SQL.Providers;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public class DeferableDelete<T> : IDeferable, IWhereQueryBuilder<T>
		where T : class
	{
		private readonly IEntityDeleteQueryBuilder<T> _queryBuilder;
		private readonly IDeleteQueryBuilder _nonGenericBuilder;
		private readonly IDataProvider _dataProvider;

		public IEntityConditionBuilder<T> Where { get => _queryBuilder.Where; set => _queryBuilder.Where = value; }
		IConditionBuilder IWhereQueryBuilder.Where { get => _nonGenericBuilder.Where; set => _nonGenericBuilder.Where = value; }

		public DeferableDelete(IEntityDeleteQueryBuilder<T> queryBuilder, IDataProvider dataProvider)
		{
			_queryBuilder = queryBuilder;
			_dataProvider = dataProvider;
		}

		public IDeferred Defer()
		{
			var result = new DeferredQuery(_dataProvider);
			result.Add(_queryBuilder.BuildQuery());
			return result;
		}

		public void Execute()
			=> Defer().Execute();

		public Task ExecuteAsync()
			=> Defer().ExecuteAsync();
	}
}
