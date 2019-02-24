using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class UpdateBuilder<T> : IEntityUpdateQueryBuilder<T>
		where T : class
	{
		private readonly EntityModel<T> _entityModel;

		public IEntityConditionBuilder<T> Where { get; set; }
		IConditionBuilder IWhereQueryBuilder.Where
		{
			get => Where;
			set => Where = (IEntityConditionBuilder<T>)value;
		}

		public IEntityFieldAssignmentBuilder<T> Assignments { get; set; }
		IFieldAssignmentBuilder IFieldAssignmentQueryBuilder.Assignments
		{
			get => Assignments;
			set => Assignments = (IEntityFieldAssignmentBuilder<T>)value;
		}

		public UpdateBuilder(Schema.Schema schema, EntityModel<T> entityModel)
		{
			_entityModel = entityModel;

			Where = new DefaultEntityConditionBuilder<T>(schema, entityModel);
			Assignments = new DefaultEntityFieldAssignmentBuilder<T>(schema, entityModel);
		}

		public UpdateBuilder(Schema.Schema schema) : this(schema, schema.GetEntityModel<T>())
		{
		}

		public QueryExpression BuildQuery()
		{
			var where = Where.Build();
			var row = Assignments.Build();

			if (where?.RequiredJoins != null && where?.RequiredJoins.Length > 0)
				ExceptionHelper.ThrowJoinsRequired();

			return QueryExpression.Update(
				QueryExpression.Table(_entityModel.Table.TableName),
				where: where?.QueryExpression,
				assignments: row
				);
		}

		public static UpdateBuilder<T> Create(Schema.Schema schema, T entity)
		{
			var entityModel = schema.GetEntityModel<T>();
			if (entityModel == null)
				ExceptionHelper.ThrowNotPresentInSchema<T>();

			return Create(schema, entityModel, entity);
		}

		public static UpdateBuilder<T> Create(Schema.Schema schema, EntityModel<T> entityModel, T entity)
		{
			var primaryKeyFields = entityModel.Fields.Where(q => q.IsPrimaryKey).ToArray();
			if (primaryKeyFields.Length == 0)
				ExceptionHelper.ThrowNoPrimaryKey<T>();

			var builder = new UpdateBuilder<T>(schema, entityModel);

			builder.Assignments.SetAll(entity);

			foreach (var primaryKeyField in primaryKeyFields)
				builder.Where.AndAlso(primaryKeyField, ComparisonOperator.AreEqual, entity);

			return builder;
		}

		public static UpdateBuilder<T> Create<TView>(Schema.Schema schema, IEntityReference<T> entityReference, TView entityView)
			where TView : class
		{
			var entityModel = schema.GetEntityModel<T>();
			if (entityModel == null)
				ExceptionHelper.ThrowNotPresentInSchema<T>();

			return Create(schema, entityModel, entityReference, entityView);
		}

		public static UpdateBuilder<T> Create<TView>(Schema.Schema schema, EntityModel<T> entityModel,
			IEntityReference<T> entityReference, TView entityView)
			where TView : class
		{
			var primaryKeyFields = entityModel.Fields.Where(q => q.IsPrimaryKey).ToArray();
			if (primaryKeyFields.Length == 0)
				ExceptionHelper.ThrowNoPrimaryKey<T>();

			var entity = entityReference.AsEntity();
			var builder = new UpdateBuilder<T>(schema, entityModel);

			builder.Assignments.SetAll(entityView);

			foreach (var primaryKeyField in primaryKeyFields)
				builder.Where.AndAlso(primaryKeyField, ComparisonOperator.AreEqual, entity);

			return builder;
		}
	}
}
