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

		private DeferredQuery Insert<TView>(IModelTranscriber<TView> transcriber, TView[] entities)
			where TView : class
		{
			var result = new DeferredQuery(_dataProvider);

			var mapBackInsertId = _serverGeneratedPrimaryKey != null;
			var generatePrimaryKey = _clientGeneratedPrimaryKey != null;
			foreach (var entity in entities)
			{
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
			}

			return result;
		}

		public IDeferred Insert(params T[] entities)
		{
			if (entities.Length < 1)
				throw new ArgumentException("Must provide at least 1 entity.", nameof(entities));

			return Insert(_entityTranscriber, entities);
		}

		public IDeferred Insert<TView>(params TView[] entityViews)
			where TView : class
		{
			if (entityViews.Length < 1)
				throw new ArgumentException("Must provide at least 1 entity.", nameof(entityViews));

			return Insert(_entityModel.GetModelTranscriber<TView>(), entityViews);
		}

		public IDeferred Insert(Action<InsertBuilder<T>> queryConfigurer)
		{
			var insertBuilder = new InsertBuilder<T>(_schema, _entityModel);
			queryConfigurer?.Invoke(insertBuilder);

			var result = new DeferredQuery(_dataProvider);
			result.Add(insertBuilder.BuildQuery());
			return result;
		}
	}
}
