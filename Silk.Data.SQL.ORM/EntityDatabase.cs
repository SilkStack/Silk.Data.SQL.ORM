using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Operations;
using Silk.Data.SQL.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM
{
	public class EntityDatabase<T> : IDatabase<T>
		where T : class
	{
		private readonly EntityModel<T> _entityModel;

		public Schema.Schema DataSchema { get; }
		public IDataProvider DataProvider { get; }

		public EntityDatabase(Schema.Schema schema, IDataProvider dataProvider)
		{
			DataProvider = dataProvider;
			DataSchema = schema;

			_entityModel = DataSchema.GetEntityModel<T>();
			if (_entityModel == null)
				throw new InvalidOperationException($"Knowledge of entity type {typeof(T).FullName} not present in provided schema.");
		}

		private InsertOperation CreateInsertOperation(IEnumerable<T> entities)
		{
			return InsertOperation.Create<T>(_entityModel, entities.ToArray());
		}

		private InsertOperation CreateInsertOperation<TView>(IEnumerable<TView> entities)
			where TView : class
		{
			return InsertOperation.Create<T, TView>(_entityModel, entities.ToArray());
		}

		private void ExecuteInsertOperation(InsertOperation operation)
		{
			if (!operation.GeneratesValuesServerSide)
			{
				DataProvider.ExecuteNonQuery(operation.GetQuery());
				return;
			}

			using (var queryResult = DataProvider.ExecuteReader(operation.GetQuery()))
				operation.ProcessResult(queryResult);
		}

		private async Task ExecuteInsertOperationAsync(InsertOperation operation)
		{
			if (!operation.GeneratesValuesServerSide)
			{
				await DataProvider.ExecuteNonQueryAsync(operation.GetQuery());
				return;
			}

			using (var queryResult = await DataProvider.ExecuteReaderAsync(operation.GetQuery()))
				await operation.ProcessResultAsync(queryResult);
		}

		public void Insert(IEnumerable<T> entities)
		{
			ExecuteInsertOperation(CreateInsertOperation(entities));
		}

		public void Insert<TView>(IEnumerable<TView> entities)
			where TView : class
		{
			ExecuteInsertOperation(CreateInsertOperation(entities));
		}

		public Task InsertAsync(IEnumerable<T> entities)
		{
			return ExecuteInsertOperationAsync(CreateInsertOperation(entities));
		}

		public Task InsertAsync<TView>(IEnumerable<TView> entities)
			where TView : class
		{
			return ExecuteInsertOperationAsync(CreateInsertOperation(entities));
		}
	}
}
