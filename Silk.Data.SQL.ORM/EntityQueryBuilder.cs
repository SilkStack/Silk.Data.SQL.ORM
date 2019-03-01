using System;
using System.Linq;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM
{
	public class EntityQueryBuilder<T> : IEntityQueryBuilder<T>
		where T : class
	{
		private readonly Schema.Schema _schema;
		private readonly EntityModel<T> _entityModel;
		private readonly EntityField<T>[] _primaryKeys;

		public EntityQueryBuilder(Schema.Schema schema)
		{
			_schema = schema;
			_entityModel = schema.GetEntityModel<T>();
			if (_entityModel == null)
				ExceptionHelper.ThrowNotPresentInSchema<T>();
			_primaryKeys = _entityModel.Fields.Where(q => q.IsPrimaryKey).ToArray();
		}

		public IEntityDeleteQueryBuilder<T> Delete(T entity)
			=> DeleteBuilder<T>.Create(_schema, _entityModel, entity);

		public IEntityDeleteQueryBuilder<T> Delete(IEntityReference<T> entityReference)
			=> DeleteBuilder<T>.Create(_schema, _entityModel, entityReference);

		public IEntityDeleteQueryBuilder<T> Delete()
			=> new DeleteBuilder<T>(_schema, _entityModel);

		public IEntityInsertQueryBuilder<T> Insert(T entity)
			=> InsertBuilder<T>.Create(_schema, _entityModel, entity);

		public IEntityInsertQueryBuilder<T> Insert<TView>(TView entityView) where TView : class
			=> InsertBuilder<T>.Create(_schema, _entityModel, entityView);

		public IEntityInsertQueryBuilder<T> Insert()
			=> new InsertBuilder<T>(_schema, _entityModel);

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

		public IEntitySelectQueryBuilder<T> Select()
			=> Select(out var _);

		public IEntitySelectQueryBuilder<T> Select(out IResultReader<T> resultReader)
		{
			var builder = new SelectBuilder<T>(_schema, _entityModel);
			resultReader = builder.Projection.AddView<T>();
			return builder;
		}

		public IEntitySelectQueryBuilder<T> Select<TView>() where TView : class
			=> Select<TView>(out var _);

		public IEntitySelectQueryBuilder<T> Select<TView>(out IResultReader<TView> resultReader) where TView : class
		{
			var builder = new SelectBuilder<T>(_schema, _entityModel);
			resultReader = builder.Projection.AddView<TView>();
			return builder;
		}

		public IEntitySelectQueryBuilder<T> Select<TExpr>(System.Linq.Expressions.Expression<Func<T, TExpr>> expression)
			=> Select(expression, out var _);

		public IEntitySelectQueryBuilder<T> Select<TExpr>(System.Linq.Expressions.Expression<Func<T, TExpr>> expression, out IResultReader<TExpr> resultReader)
		{
			var builder = new SelectBuilder<T>(_schema, _entityModel);
			resultReader = builder.Projection.AddField(expression);
			return builder;
		}

		public IEntityUpdateQueryBuilder<T> Update(T entity)
			=> UpdateBuilder<T>.Create(_schema, _entityModel, entity);

		public IEntityUpdateQueryBuilder<T> Update<TView>(IEntityReference<T> entityReference, TView view) where TView : class
			=> UpdateBuilder<T>.Create(_schema, _entityModel, entityReference, view);

		public IEntityUpdateQueryBuilder<T> Update()
			=> new UpdateBuilder<T>(_schema, _entityModel);
	}
}
