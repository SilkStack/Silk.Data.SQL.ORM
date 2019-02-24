using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Providers;
using Silk.Data.SQL.Queries;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM
{
	public class EntityTable<T> : IEntityTable<T>
		where T : class
	{
		private readonly Schema.Schema _schema;
		private readonly EntityModel<T> _entityModel;
		private readonly IDataProvider _dataProvider;

		public EntityTable(Schema.Schema schema, EntityModel<T> entityModel, IDataProvider dataProvider)
		{
			_schema = schema;
			_entityModel = entityModel;
			_dataProvider = dataProvider;
		}

		public EntityTable(Schema.Schema schema, IDataProvider dataProvider) :
			this(schema, schema.GetEntityModel<T>(), dataProvider)
		{
		}

		public IDeferred CreateTable()
		{
			var deferredQuery = new DeferredQuery(_dataProvider);

			deferredQuery.Add(new CreateTableQueryBuilder<T>(_entityModel).BuildQuery());

			return deferredQuery;
		}

		public IDeferred DropTable()
		{
			var deferredQuery = new DeferredQuery(_dataProvider);

			deferredQuery.Add(QueryExpression.DropTable(_entityModel.Table.TableName));

			return deferredQuery;
		}

		public IDeferred TableExists(out DeferredResult<bool> tableExists)
		{
			var resultSource = new DeferredResultSource<bool>();
			tableExists = resultSource.DeferredResult;

			var deferredQuery = new DeferredQuery(_dataProvider);

			deferredQuery.Add(
				QueryExpression.TableExists(_entityModel.Table.TableName),
				new TableExistsResultProcessor(resultSource)
				);

			return deferredQuery;
		}

		private class TableExistsResultProcessor : IQueryResultProcessor
		{
			private readonly DeferredResultSource<bool> _deferredResultSource;

			public TableExistsResultProcessor(DeferredResultSource<bool> deferredResultSource)
			{
				_deferredResultSource = deferredResultSource;
			}

			public void HandleFailure()
			{
				if (_deferredResultSource.DeferredResult.TaskHasRun)
					return;
				_deferredResultSource.SetFailed();
			}

			public void ProcessResult(QueryResult queryResult)
			{
				if (!queryResult.HasRows || !queryResult.Read())
					_deferredResultSource.SetResult(false);

				_deferredResultSource.SetResult(queryResult.GetInt32(0) > 0);
			}

			public async Task ProcessResultAsync(QueryResult queryResult)
			{
				if (!queryResult.HasRows || !await queryResult.ReadAsync())
					_deferredResultSource.SetResult(false);

				_deferredResultSource.SetResult(queryResult.GetInt32(0) > 0);
			}
		}
	}
}
