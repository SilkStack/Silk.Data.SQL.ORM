using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Providers;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public class DeferableInsert<T> : IDeferable, IFieldAssignmentQueryBuilder<T>
		where T : class
	{
		private readonly IEntityInsertQueryBuilder<T> _queryBuilder;
		private readonly IInsertQueryBuilder _nonGenericBuilder;
		private readonly IDataProvider _dataProvider;
		private readonly QueryExpression _followUpQuery;
		private readonly IQueryResultProcessor _followUpProcessor;

		public IEntityFieldAssignmentBuilder<T> Assignments { get => _queryBuilder.Assignments; set => _queryBuilder.Assignments = value; }
		IFieldAssignmentBuilder IFieldAssignmentQueryBuilder.Assignments { get => _nonGenericBuilder.Assignments; set => _nonGenericBuilder.Assignments = value; }

		public DeferableInsert(IEntityInsertQueryBuilder<T> queryBuilder, IDataProvider dataProvider)
		{
			_queryBuilder = queryBuilder;
			_nonGenericBuilder = queryBuilder;
			_dataProvider = dataProvider;
		}

		public DeferableInsert(IEntityInsertQueryBuilder<T> queryBuilder, IDataProvider dataProvider,
			QueryExpression followUpQuery, IQueryResultProcessor followUpProcessor) :
			this(queryBuilder, dataProvider)
		{
			_followUpQuery = followUpQuery;
			_followUpProcessor = followUpProcessor;
		}

		public IDeferred Defer()
		{
			var result = new DeferredQuery(_dataProvider);
			result.Add(_queryBuilder.BuildQuery());
			if (_followUpQuery != null && _followUpProcessor != null)
				result.Add(_followUpQuery, _followUpProcessor);
			return result;
		}

		public void Execute()
			=> Defer().Execute();

		public Task ExecuteAsync()
			=> Defer().ExecuteAsync();
	}
}
