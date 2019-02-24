using System;
using System.Linq;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Queries
{
	public class DeleteBuilder<T> : IEntityDeleteQueryBuilder<T>
		where T : class
	{
		private readonly EntityModel<T> _entityModel;

		public IEntityConditionBuilder<T> Where { get; set; }
		IConditionBuilder IWhereQueryBuilder.Where
		{
			get => Where;
			set => Where = (IEntityConditionBuilder<T>)value;
		}

		public DeleteBuilder(Schema.Schema schema, EntityModel<T> entityModel)
		{
			_entityModel = entityModel;
			Where = new DefaultEntityConditionBuilder<T>(schema, entityModel);
		}

		public DeleteBuilder(Schema.Schema schema) : this(schema, schema.GetEntityModel<T>())
		{
		}

		public QueryExpression BuildQuery()
		{
			var where = Where.Build();

			if (where?.RequiredJoins != null && where?.RequiredJoins.Length > 0)
				ExceptionHelper.ThrowJoinsRequired();

			return QueryExpression.Delete(
				QueryExpression.Table(_entityModel.Table.TableName), where?.QueryExpression
				);
		}

		public static DeleteBuilder<T> Create(Schema.Schema schema, T entity)
		{
			var entityModel = schema.GetEntityModel<T>();
			if (entityModel == null)
				ExceptionHelper.ThrowNotPresentInSchema<T>();

			return Create(schema, entityModel, entity);
		}

		public static DeleteBuilder<T> Create(Schema.Schema schema, EntityModel<T> entityModel, T entity)
		{
			var primaryKeyFields = entityModel.Fields.Where(q => q.IsPrimaryKey).ToArray();
			if (primaryKeyFields.Length == 0)
				ExceptionHelper.ThrowNoPrimaryKey<T>();

			var builder = new DeleteBuilder<T>(schema, entityModel);

			foreach (var primaryKeyField in primaryKeyFields)
				builder.Where.AndAlso(primaryKeyField, ComparisonOperator.AreEqual, entity);

			return builder;
		}

		public static DeleteBuilder<T> Create(Schema.Schema schema, IEntityReference<T> entity)
			=> Create(schema, entity.AsEntity());

		public static DeleteBuilder<T> Create(Schema.Schema schema, EntityModel<T> entityModel, IEntityReference<T> entity)
			=> Create(schema, entityModel, entity.AsEntity());
	}
}
