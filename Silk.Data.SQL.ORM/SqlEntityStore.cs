using System;
using System.Linq;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Providers;

namespace Silk.Data.SQL.ORM
{
	public class SqlEntityStore<T> : ISqlEntityStore<T>
		where T : class
	{
		private readonly Schema.Schema _schema;
		private readonly EntityModel<T> _entityModel;
		private readonly IDataProvider _dataProvider;

		private readonly EntityField<T> _clientGeneratedPrimaryKey;
		private readonly EntityField<T> _serverGeneratedPrimaryKey;

		private readonly IModelTranscriber<T> _entityTranscriber;

		public SqlEntityStore(Schema.Schema schema, IDataProvider dataProvider)
		{
			_schema = schema;
			_entityModel = schema.GetEntityModel<T>();
			_dataProvider = dataProvider;

			if (_entityModel == null)
				ExceptionHelper.ThrowNotPresentInSchema<T>();

			_clientGeneratedPrimaryKey = _entityModel.Fields.FirstOrDefault(q => q.IsPrimaryKey && !q.IsSeverGenerated);
			_serverGeneratedPrimaryKey = _entityModel.Fields.FirstOrDefault(q => q.IsPrimaryKey && q.IsSeverGenerated);

			_entityTranscriber = _entityModel.GetModelTranscriber(_entityModel.TypeModel);
		}

		private void AttemptWriteToObject<TView, TData>(TView obj, TData data, EntityField<T> entityField,
			IModelTranscriber<TView> transcriber)
			where TView : class
		{
			var helper = transcriber.SchemaToTypeHelpers.FirstOrDefault(q => q.From == entityField);
			if (helper == null)
				return;

			helper.WriteValueToInstance(obj, data);
		}

		private DeferredQuery Insert<TView>(IModelTranscriber<TView> transcriber, TView entity)
			where TView : class
		{
			var result = new DeferredQuery(_dataProvider);

			var mapBackInsertId = _serverGeneratedPrimaryKey != null;
			var generatePrimaryKey = _clientGeneratedPrimaryKey != null;
			var insertBuilder = InsertBuilder<T>.Create(_schema, _entityModel, entity);

			if (generatePrimaryKey)
			{
				var newId = Guid.NewGuid();
				insertBuilder.Assignments.Set(_clientGeneratedPrimaryKey, newId);
				AttemptWriteToObject(entity, newId, _clientGeneratedPrimaryKey, transcriber);
			}

			result.Add(insertBuilder.BuildQuery());

			if (mapBackInsertId)
			{

			}

			return result;
		}

		public IDeferred Insert(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			return Insert(_entityTranscriber, entity);
		}

		public IDeferred Insert<TView>(TView entityView)
			where TView : class
		{
			if (entityView == null)
				throw new ArgumentNullException(nameof(entityView));

			return Insert(_entityModel.GetModelTranscriber<TView>(), entityView);
		}

		public IDeferred Insert(Action<InsertBuilder<T>> queryConfigurer)
		{
			var insertBuilder = new InsertBuilder<T>(_schema, _entityModel);
			queryConfigurer?.Invoke(insertBuilder);

			var result = new DeferredQuery(_dataProvider);
			result.Add(insertBuilder.BuildQuery());
			return result;
		}

		public IDeferred Delete(T entity)
		{
			var result = new DeferredQuery(_dataProvider);
			result.Add(
				DeleteBuilder<T>.Create(_schema, _entityModel, entity).BuildQuery()
				);
			return result;
		}

		public IDeferred Delete(IEntityReference<T> entityReference)
		{
			var result = new DeferredQuery(_dataProvider);
			result.Add(
				DeleteBuilder<T>.Create(_schema, _entityModel, entityReference).BuildQuery()
				);
			return result;
		}

		public IDeferred Delete(Action<DeleteBuilder<T>> queryConfigurer)
		{
			var deleteBuilder = new DeleteBuilder<T>(_schema, _entityModel);
			queryConfigurer?.Invoke(deleteBuilder);

			var result = new DeferredQuery(_dataProvider);
			result.Add(deleteBuilder.BuildQuery());
			return result;
		}

		public IDeferred Update(T entity)
		{
			var result = new DeferredQuery(_dataProvider);
			result.Add(
				UpdateBuilder<T>.Create(_schema, _entityModel, entity).BuildQuery()
				);
			return result;
		}

		public IDeferred Update<TView>(IEntityReference<T> entityReference, TView view)
			where TView : class
		{
			var result = new DeferredQuery(_dataProvider);
			result.Add(
				UpdateBuilder<T>.Create(_schema, _entityModel, entityReference, view).BuildQuery()
				);
			return result;
		}

		public IDeferred Update(Action<UpdateBuilder<T>> queryConfigurer)
		{
			var updateBuilder = new UpdateBuilder<T>(_schema, _entityModel);
			queryConfigurer?.Invoke(updateBuilder);

			var result = new DeferredQuery(_dataProvider);
			result.Add(updateBuilder.BuildQuery());
			return result;
		}
	}
}
