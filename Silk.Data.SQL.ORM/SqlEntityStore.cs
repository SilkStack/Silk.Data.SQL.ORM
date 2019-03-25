using System;
using System.Linq;
using System.Threading.Tasks;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.Mapping;
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
		private readonly EntityModel<T> _entityModel;
		private readonly IDataProvider _dataProvider;

		private readonly EntityField<T> _clientGeneratedPrimaryKey;
		private readonly EntityField<T> _serverGeneratedPrimaryKey;
		private readonly IEntityView<T> _entityTranscriber;
		private readonly ITypeInstanceFactory _typeInstanceFactory;
		private readonly IReaderWriterFactory<TypeModel, PropertyInfoField> _typeReaderWriterFactory;

		public IEntityQueryBuilder<T> QueryBuilder { get; set; }

		public SqlEntityStore(Schema.Schema schema, IDataProvider dataProvider)
		{
			_entityModel = schema.GetEntityModel<T>();
			_dataProvider = dataProvider;

			if (_entityModel == null)
				ExceptionHelper.ThrowNotPresentInSchema<T>();

			QueryBuilder = new EntityQueryBuilder<T>(schema);

			_clientGeneratedPrimaryKey = _entityModel.Fields.FirstOrDefault(q => q.IsPrimaryKey && !q.IsSeverGenerated);
			_serverGeneratedPrimaryKey = _entityModel.Fields.FirstOrDefault(q => q.IsPrimaryKey && q.IsSeverGenerated);

			_entityTranscriber = _entityModel.GetEntityView(_entityModel.TypeModel);
		}

		public SqlEntityStore(Schema.Schema schema, IDataProvider dataProvider, ITypeInstanceFactory typeInstanceFactory,
			IReaderWriterFactory<TypeModel, PropertyInfoField> typeReaderWriterFactory) :
			this(schema, dataProvider)
		{
			_typeInstanceFactory = typeInstanceFactory;
			_typeReaderWriterFactory = typeReaderWriterFactory;
		}

		private static void AttemptWriteToObject<TView, TData>(TView obj, TData data, EntityField<T> entityField,
			IEntityView<TView> entityView)
			where TView : class
		{
			var intersectedFields = entityView.EntityToClassIntersection
				.IntersectedFields.FirstOrDefault(q => q.LeftField == entityField);
			if (intersectedFields == null)
				return;

			var writer = new ObjectGraphReaderWriter<TView>(obj);
			writer.Write(intersectedFields.RightPath, data);
		}

		private DeferableInsert<T> Insert<TView>(IEntityView<TView> transcriber, TView entity)
			where TView : class
		{
			var mapBackInsertId = _serverGeneratedPrimaryKey != null;
			var generatePrimaryKey = _clientGeneratedPrimaryKey != null;
			var insertBuilder = QueryBuilder.Insert(entity);

			if (generatePrimaryKey)
			{
				var newId = Guid.NewGuid();
				insertBuilder.Assignments.Set(_clientGeneratedPrimaryKey, newId);
				AttemptWriteToObject(entity, newId, _clientGeneratedPrimaryKey, transcriber);
			}

			if (!mapBackInsertId)
			{
				return new DeferableInsert<T>(
					insertBuilder, _dataProvider
				);
			}

			var primaryKeyIntersectedFields = transcriber.EntityToClassIntersection.IntersectedFields
				.FirstOrDefault(q => q.LeftField == _serverGeneratedPrimaryKey);
			if (primaryKeyIntersectedFields == null)
			{
				return new DeferableInsert<T>(
					insertBuilder, _dataProvider
				);
			}

			return new DeferableInsert<T>(
				insertBuilder, _dataProvider,
				QueryExpression.Select(
					QueryExpression.Alias(QueryExpression.LastInsertIdFunction(), _serverGeneratedPrimaryKey.ProjectionAlias),
					from: QueryExpression.Table(_entityModel.Table.TableName)
				),
				new MapLastIdResultProcessor<TView>(
					entity, primaryKeyIntersectedFields
					)
				);
		}

		public DeferableInsert<T> Insert(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			return Insert(_entityTranscriber, entity);
		}

		public DeferableInsert<T> Insert<TView>(TView entityView)
			where TView : class
		{
			if (entityView == null)
				throw new ArgumentNullException(nameof(entityView));

			return Insert(_entityModel.GetEntityView<TView>(), entityView);
		}

		public DeferableInsert<T> Insert()
			=> new DeferableInsert<T>(QueryBuilder.Insert(), _dataProvider);

		public DeferableDelete<T> Delete(T entity)
			=> new DeferableDelete<T>(QueryBuilder.Delete(entity), _dataProvider);

		public DeferableDelete<T> Delete(IEntityReference<T> entityReference)
			=> new DeferableDelete<T>(QueryBuilder.Delete(entityReference), _dataProvider);

		public DeferableDelete<T> Delete()
			=> new DeferableDelete<T>(QueryBuilder.Delete(), _dataProvider);

		public DeferableUpdate<T> Update(T entity)
			=> new DeferableUpdate<T>(QueryBuilder.Update(entity), _dataProvider);

		public DeferableUpdate<T> Update<TView>(IEntityReference<T> entityReference, TView view)
			where TView : class
			=> new DeferableUpdate<T>(QueryBuilder.Update(entityReference, view), _dataProvider);

		public DeferableUpdate<T> Update()
			=> new DeferableUpdate<T>(QueryBuilder.Update(), _dataProvider);

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

		public SingleDeferableSelect<T, T> Select(IEntityReference<T> entityReference)
		{
			var builder = QueryBuilder.Select(entityReference, out var resultReader);
			AttachCustomFactories(resultReader);

			return new SingleDeferableSelect<T, T>(
				builder, _dataProvider, resultReader
				);
		}

		public SingleDeferableSelect<T, TView> Select<TView>(IEntityReference<T> entityReference)
			where TView : class
		{
			var builder = QueryBuilder.Select<TView>(entityReference, out var resultReader);
			AttachCustomFactories(resultReader);

			return new SingleDeferableSelect<T, TView>(
				builder, _dataProvider, resultReader
				);
		}

		public MultipleDeferableSelect<T, T> Select()
		{
			var builder = QueryBuilder.Select(out var resultReader);
			AttachCustomFactories(resultReader);

			return new MultipleDeferableSelect<T, T>(
				builder, _dataProvider, resultReader
				);
		}

		public MultipleDeferableSelect<T, TView> Select<TView>()
			where TView : class
		{
			var builder = QueryBuilder.Select<TView>(out var resultReader);
			AttachCustomFactories(resultReader);

			return new MultipleDeferableSelect<T, TView>(
				builder, _dataProvider, resultReader
				);
		}

		public MultipleDeferableSelect<T, TExpr> Select<TExpr>(System.Linq.Expressions.Expression<Func<T, TExpr>> expression)
		{
			var builder = QueryBuilder.Select(expression, out var resultReader);

			return new MultipleDeferableSelect<T, TExpr>(
				builder, _dataProvider, resultReader
				);
		}

		private class MapLastIdResultProcessor<TView> : IQueryResultProcessor
			where TView : class
		{
			private readonly TView _view;
			private readonly IntersectedFields<EntityModel, EntityField, TypeModel, PropertyInfoField> _primaryKeyIntersectedFields;

			public MapLastIdResultProcessor(TView view,
				IntersectedFields<EntityModel,EntityField,TypeModel,PropertyInfoField> primaryKeyIntersectedFields)
			{
				_view = view;
				_primaryKeyIntersectedFields = primaryKeyIntersectedFields;
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
				var writer = new ObjectGraphReaderWriter<TView>(_view);
				var value = (object)queryResult.GetInt32(0);
				writer.Write(_primaryKeyIntersectedFields.RightPath, value);
			}
		}
	}
}
