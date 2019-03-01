using System;
using System.Linq;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Providers;

namespace Silk.Data.SQL.ORM
{
	public class EntityQueryBuilder<T> : IEntityQueryBuilder<T>
		where T : class
	{
		private readonly Schema.Schema _schema;
		private readonly EntityModel<T> _entityModel;
		private readonly IDataProvider _dataProvider;
		private readonly EntityField<T>[] _primaryKeys;

		public EntityQueryBuilder(Schema.Schema schema, IDataProvider dataProvider)
		{
			_schema = schema;
			_dataProvider = dataProvider;
			_entityModel = schema.GetEntityModel<T>();
			if (_entityModel == null)
				ExceptionHelper.ThrowNotPresentInSchema<T>();
			_primaryKeys = _entityModel.Fields.Where(q => q.IsPrimaryKey).ToArray();
		}

		public IEntityDeleteQueryBuilder<T> Delete(T entity)
			=> DeleteBuilder<T>.Create(_schema, _entityModel, entity);

		public IEntityDeleteQueryBuilder<T> Delete(IEntityReference<T> entityReference)
			=> DeleteBuilder<T>.Create(_schema, _entityModel, entityReference);

		public IEntityDeleteQueryBuilder<T> Delete(Action<IEntityDeleteQueryBuilder<T>> queryConfigurer)
		{
			var deleteBuilder = new DeleteBuilder<T>(_schema, _entityModel);
			queryConfigurer?.Invoke(deleteBuilder);
			return deleteBuilder;
		}

		public IEntityInsertQueryBuilder<T> Insert(T entity)
			=> InsertBuilder<T>.Create(_schema, _entityModel, entity);

		public IEntityInsertQueryBuilder<T> Insert<TView>(TView entityView) where TView : class
			=> InsertBuilder<T>.Create(_schema, _entityModel, entityView);

		public IEntityInsertQueryBuilder<T> Insert(Action<IEntityInsertQueryBuilder<T>> queryConfigurer)
		{
			var insertBuilder = new InsertBuilder<T>(_schema, _entityModel);
			queryConfigurer?.Invoke(insertBuilder);
			return insertBuilder;
		}

		public IEntitySelectQueryBuilder<T> Select(IEntityReference<T> entityReference)
			=> Select(entityReference, out var _);

		public IEntitySelectQueryBuilder<T> Select(IEntityReference<T> entityReference, out IResultReader<T> resultReader)
		{
			if (_primaryKeys.Length == 0)
				ExceptionHelper.ThrowNoPrimaryKey<T>();
			var entity = entityReference.AsEntity();
			var builder = new SelectBuilder<T>(_schema, _entityModel);
			foreach (var field in _primaryKeys)
				builder.Where.AndAlso(field, ComparisonOperator.AreEqual, entity);
			builder.Range.Limit(1);
			resultReader = builder.Projection.AddView<T>();
			return builder;
		}

		public IEntitySelectQueryBuilder<T> Select<TView>(IEntityReference<T> entityReference) where TView : class
			=> Select<TView>(entityReference, out var _);

		public IEntitySelectQueryBuilder<T> Select<TView>(IEntityReference<T> entityReference, out IResultReader<TView> resultReader) where TView : class
		{
			if (_primaryKeys.Length == 0)
				ExceptionHelper.ThrowNoPrimaryKey<T>();
			var entity = entityReference.AsEntity();
			var builder = new SelectBuilder<T>(_schema, _entityModel);
			foreach (var field in _primaryKeys)
				builder.Where.AndAlso(field, ComparisonOperator.AreEqual, entity);
			builder.Range.Limit(1);
			resultReader = builder.Projection.AddView<TView>();
			return builder;
		}

		public IEntitySelectQueryBuilder<T> Select(Action<IEntitySelectQueryBuilder<T>> query)
			=> Select(query, out var _);

		public IEntitySelectQueryBuilder<T> Select(Action<IEntitySelectQueryBuilder<T>> query, out IResultReader<T> resultReader)
		{
			var builder = new SelectBuilder<T>(_schema, _entityModel);
			query?.Invoke(builder);
			resultReader = builder.Projection.AddView<T>();
			return builder;
		}

		public IEntitySelectQueryBuilder<T> Select<TView>(Action<IEntitySelectQueryBuilder<T>> query) where TView : class
			=> Select<TView>(query, out var _);

		public IEntitySelectQueryBuilder<T> Select<TView>(Action<IEntitySelectQueryBuilder<T>> query, out IResultReader<TView> resultReader) where TView : class
		{
			var builder = new SelectBuilder<T>(_schema, _entityModel);
			query?.Invoke(builder);
			resultReader = builder.Projection.AddView<TView>();
			return builder;
		}

		public IEntitySelectQueryBuilder<T> Select<TExpr>(System.Linq.Expressions.Expression<Func<T, TExpr>> expression, Action<IEntitySelectQueryBuilder<T>> query)
			=> Select(expression, query, out var _);

		public IEntitySelectQueryBuilder<T> Select<TExpr>(System.Linq.Expressions.Expression<Func<T, TExpr>> expression, Action<IEntitySelectQueryBuilder<T>> query, out IResultReader<TExpr> resultReader)
		{
			var builder = new SelectBuilder<T>(_schema, _entityModel);
			query?.Invoke(builder);
			resultReader = builder.Projection.AddField(expression);
			return builder;
		}

		public IEntityUpdateQueryBuilder<T> Update(T entity)
			=> UpdateBuilder<T>.Create(_schema, _entityModel, entity);

		public IEntityUpdateQueryBuilder<T> Update<TView>(IEntityReference<T> entityReference, TView view) where TView : class
			=> UpdateBuilder<T>.Create(_schema, _entityModel, entityReference, view);

		public IEntityUpdateQueryBuilder<T> Update(Action<IEntityUpdateQueryBuilder<T>> queryConfigurer)
		{
			var updateBuilder = new UpdateBuilder<T>(_schema, _entityModel);
			queryConfigurer?.Invoke(updateBuilder);
			return updateBuilder;
		}
	}
}
