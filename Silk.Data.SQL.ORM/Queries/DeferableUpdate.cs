using Silk.Data.SQL.Providers;
using System;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public class DeferableUpdate<T> : IDeferable, IWhereQueryBuilder<T>, IFieldAssignmentQueryBuilder<T>
		where T : class
	{
		private readonly IEntityUpdateQueryBuilder<T> _queryBuilder;
		private readonly IUpdateQueryBuilder _nonGenericBuilder;
		private readonly IDataProvider _dataProvider;

		public IEntityFieldAssignmentBuilder<T> Assignments { get => _queryBuilder.Assignments; set => _queryBuilder.Assignments = value; }
		IFieldAssignmentBuilder IFieldAssignmentQueryBuilder.Assignments { get => _nonGenericBuilder.Assignments; set => _nonGenericBuilder.Assignments = value; }

		public IEntityConditionBuilder<T> Where { get => _queryBuilder.Where; set => _queryBuilder.Where = value; }
		IConditionBuilder IWhereQueryBuilder.Where { get => _nonGenericBuilder.Where; set => _nonGenericBuilder.Where = value; }

		public DeferableUpdate(IEntityUpdateQueryBuilder<T> queryBuilder, IDataProvider dataProvider)
		{
			_queryBuilder = queryBuilder;
			_nonGenericBuilder = queryBuilder;
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
