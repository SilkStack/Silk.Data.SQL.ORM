using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Providers;
using Silk.Data.SQL.Queries;

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
		private readonly EntityField<T>[] _primaryKeys;
		private readonly IModelTranscriber<T> _entityTranscriber;
		private readonly ITypeInstanceFactory _typeInstanceFactory;
		private readonly IReaderWriterFactory<TypeModel, PropertyInfoField> _typeReaderWriterFactory;

		public IEntityQueryBuilder<T> QueryBuilder { get; set; }

		public SqlEntityStore(Schema.Schema schema, IDataProvider dataProvider)
		{
			_schema = schema;
			_entityModel = schema.GetEntityModel<T>();
			_dataProvider = dataProvider;

			if (_entityModel == null)
				ExceptionHelper.ThrowNotPresentInSchema<T>();

			QueryBuilder = new EntityQueryBuilder<T>(schema);

			_clientGeneratedPrimaryKey = _entityModel.Fields.FirstOrDefault(q => q.IsPrimaryKey && !q.IsSeverGenerated);
			_serverGeneratedPrimaryKey = _entityModel.Fields.FirstOrDefault(q => q.IsPrimaryKey && q.IsSeverGenerated);
			_primaryKeys = _entityModel.Fields.Where(q => q.IsPrimaryKey).ToArray();

			_entityTranscriber = _entityModel.GetModelTranscriber(_entityModel.TypeModel);
		}

		public SqlEntityStore(Schema.Schema schema, IDataProvider dataProvider, ITypeInstanceFactory typeInstanceFactory,
			IReaderWriterFactory<TypeModel, PropertyInfoField> typeReaderWriterFactory) :
			this(schema, dataProvider)
		{
			_typeInstanceFactory = typeInstanceFactory;
			_typeReaderWriterFactory = typeReaderWriterFactory;
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
			var insertBuilder = QueryBuilder.Insert(entity);

			if (generatePrimaryKey)
			{
				var newId = Guid.NewGuid();
				insertBuilder.Assignments.Set(_clientGeneratedPrimaryKey, newId);
				AttemptWriteToObject(entity, newId, _clientGeneratedPrimaryKey, transcriber);
			}

			result.Add(insertBuilder.BuildQuery());

			if (mapBackInsertId)
			{
				var idFieldHelper = transcriber.SchemaToTypeHelpers.FirstOrDefault(q => q.From == _serverGeneratedPrimaryKey);
				if (idFieldHelper != null)
				{
					result.Add(
						QueryExpression.Select(
							QueryExpression.Alias(QueryExpression.LastInsertIdFunction(), _serverGeneratedPrimaryKey.ProjectionAlias),
							from: QueryExpression.Table(_entityModel.Table.TableName)
						),
						new MapLastIdResultProcessor<TView>(
							entity, idFieldHelper
							));
				}
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

		public IDeferred Insert(Action<IEntityInsertQueryBuilder<T>> queryConfigurer)
		{
			var result = new DeferredQuery(_dataProvider);
			var query = QueryBuilder.Insert();
			queryConfigurer?.Invoke(query);
			result.Add(query.BuildQuery());
			return result;
		}

		public IDeferred Delete(T entity)
		{
			var result = new DeferredQuery(_dataProvider);
			result.Add(
				QueryBuilder.Delete(entity).BuildQuery()
				);
			return result;
		}

		public IDeferred Delete(IEntityReference<T> entityReference)
		{
			var result = new DeferredQuery(_dataProvider);
			result.Add(
				QueryBuilder.Delete(entityReference).BuildQuery()
				);
			return result;
		}

		public IDeferred Delete(Action<IEntityDeleteQueryBuilder<T>> queryConfigurer)
		{
			var result = new DeferredQuery(_dataProvider);
			var query = QueryBuilder.Delete();
			queryConfigurer?.Invoke(query);
			result.Add(query.BuildQuery());
			return result;
		}

		public IDeferred Update(T entity)
		{
			var result = new DeferredQuery(_dataProvider);
			result.Add(
				QueryBuilder.Update(entity).BuildQuery()
				);
			return result;
		}

		public IDeferred Update<TView>(IEntityReference<T> entityReference, TView view)
			where TView : class
		{
			var result = new DeferredQuery(_dataProvider);
			result.Add(
				QueryBuilder.Update(entityReference, view).BuildQuery()
				);
			return result;
		}

		public IDeferred Update(Action<IEntityUpdateQueryBuilder<T>> queryConfigurer)
		{
			var result = new DeferredQuery(_dataProvider);
			var query = QueryBuilder.Update();
			queryConfigurer?.Invoke(query);
			result.Add(query.BuildQuery());
			return result;
		}

		private void AttachCustomFactories<TView>(IResultReader<TView> resultReader)
			where TView : class
		{
			var mappingReader = resultReader as MappingReader<TView>;
			if (mappingReader == null)
				return;

			if (_typeInstanceFactory != null)
				mappingReader.TypeInstanceFactory = _typeInstanceFactory;

			if (_typeReaderWriterFactory != null)
				mappingReader.TypeReaderWriterFactory = _typeReaderWriterFactory;
		}

		public IDeferred Select(IEntityReference<T> entityReference, out DeferredResult<T> entityResult)
		{
			var builder = QueryBuilder.Select(entityReference, out var resultReader);
			AttachCustomFactories(resultReader);

			var resultSource = new DeferredResultSource<T>();
			entityResult = resultSource.DeferredResult;

			var result = new DeferredQuery(_dataProvider);
			result.Add(builder.BuildQuery(), new SingleMappedResultProcessor<T>(
				resultReader,
				resultSource
				));

			return result;
		}

		public IDeferred Select<TView>(IEntityReference<T> entityReference, out DeferredResult<TView> viewResult)
			where TView : class
		{
			var builder = QueryBuilder.Select<TView>(entityReference, out var resultReader);
			AttachCustomFactories(resultReader);

			var resultSource = new DeferredResultSource<TView>();
			viewResult = resultSource.DeferredResult;

			var result = new DeferredQuery(_dataProvider);
			result.Add(builder.BuildQuery(), new SingleMappedResultProcessor<TView>(
				resultReader,
				resultSource
				));

			return result;
		}

		public IDeferred Select(Action<IEntitySelectQueryBuilder<T>> query, out DeferredResult<List<T>> entitiesResult)
		{
			var builder = QueryBuilder.Select(out var resultReader);
			query?.Invoke(builder);
			AttachCustomFactories(resultReader);

			var resultSource = new DeferredResultSource<List<T>>();
			entitiesResult = resultSource.DeferredResult;

			var result = new DeferredQuery(_dataProvider);
			result.Add(builder.BuildQuery(), new ManyMappedResultProcessor<T>(
				resultReader,
				resultSource
				));

			return result;
		}

		public IDeferred Select<TView>(Action<IEntitySelectQueryBuilder<T>> query, out DeferredResult<List<TView>> viewsResult)
			where TView : class
		{
			var builder = QueryBuilder.Select<TView>(out var resultReader);
			query?.Invoke(builder);
			AttachCustomFactories(resultReader);

			var resultSource = new DeferredResultSource<List<TView>>();
			viewsResult = resultSource.DeferredResult;

			var result = new DeferredQuery(_dataProvider);
			result.Add(builder.BuildQuery(), new ManyMappedResultProcessor<TView>(
				resultReader,
				resultSource
				));

			return result;
		}

		public IDeferred Select<TExpr>(System.Linq.Expressions.Expression<Func<T, TExpr>> expression,
			Action<IEntitySelectQueryBuilder<T>> query, out DeferredResult<List<TExpr>> exprsResult)
		{
			var builder = QueryBuilder.Select(expression, out var resultReader);
			query?.Invoke(builder);

			var resultSource = new DeferredResultSource<List<TExpr>>();
			exprsResult = resultSource.DeferredResult;

			var result = new DeferredQuery(_dataProvider);
			result.Add(builder.BuildQuery(), new ManyMappedResultProcessor<TExpr>(
				resultReader,
				resultSource
				));

			return result;
		}

		private class MapLastIdResultProcessor<TView> : IQueryResultProcessor
			where TView : class
		{
			private readonly TView _view;
			private readonly TypeModelHelper<TView> _fieldHelper;

			public MapLastIdResultProcessor(TView view, TypeModelHelper<TView> fieldHelper)
			{
				_view = view;
				_fieldHelper = fieldHelper;
			}

			public void HandleFailure()
			{
			}

			public void ProcessResult(QueryResult queryResult)
			{
				if (!queryResult.HasRows || !queryResult.Read())
					return;
				MapValue(queryResult);
			}

			public async Task ProcessResultAsync(QueryResult queryResult)
			{
				if (!queryResult.HasRows || !await queryResult.ReadAsync())
					return;
				MapValue(queryResult);
			}

			private void MapValue(QueryResult queryResult)
			{
				_fieldHelper.CopyResultToObject(queryResult, _view);
			}
		}

		private class SingleMappedResultProcessor<TResult> : IQueryResultProcessor
		{
			private readonly IResultReader<TResult> _resultReader;
			private readonly DeferredResultSource<TResult> _resultSource;

			public SingleMappedResultProcessor(
				IResultReader<TResult> resultReader,
				DeferredResultSource<TResult> deferredResultSource
				)
			{
				_resultReader = resultReader;
				_resultSource = deferredResultSource;
			}

			public void HandleFailure()
			{
				if (_resultSource.DeferredResult.TaskHasRun)
					return;
				_resultSource.SetFailed();
			}

			public void ProcessResult(QueryResult queryResult)
			{
				if (!queryResult.HasRows || !queryResult.Read())
				{
					_resultSource.SetResult(default(TResult));
					return;
				}

				_resultSource.SetResult(_resultReader.Read(queryResult));
			}

			public async Task ProcessResultAsync(QueryResult queryResult)
			{
				if (!queryResult.HasRows || !await queryResult.ReadAsync())
				{
					_resultSource.SetResult(default(TResult));
					return;
				}

				_resultSource.SetResult(_resultReader.Read(queryResult));
			}
		}

		private class ManyMappedResultProcessor<TResult> : IQueryResultProcessor
		{
			private readonly IResultReader<TResult> _resultReader;
			private readonly DeferredResultSource<List<TResult>> _resultSource;

			public ManyMappedResultProcessor(
				IResultReader<TResult> resultReader,
				DeferredResultSource<List<TResult>> deferredResultSource
				)
			{
				_resultReader = resultReader;
				_resultSource = deferredResultSource;
			}

			public void HandleFailure()
			{
				if (_resultSource.DeferredResult.TaskHasRun)
					return;
				_resultSource.SetFailed();
			}

			public void ProcessResult(QueryResult queryResult)
			{
				var result = new List<TResult>();
				while (queryResult.Read())
					result.Add(_resultReader.Read(queryResult));
				_resultSource.SetResult(result);
			}

			public async Task ProcessResultAsync(QueryResult queryResult)
			{
				var result = new List<TResult>();
				while (await queryResult.ReadAsync())
					result.Add(_resultReader.Read(queryResult));
				_resultSource.SetResult(result);
			}
		}
	}
}
