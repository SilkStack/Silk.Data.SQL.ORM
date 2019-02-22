using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;

namespace Silk.Data.SQL.ORM.Queries
{
	public class InsertBuilder : IQueryBuilder, IInsertQueryBuilder
	{
		public virtual IFieldAssignmentBuilder Assignments { get; set; } = new DefaultFieldAssignmentBuilder();

		public QueryExpression BuildQuery()
		{
			throw new System.NotImplementedException();
		}
	}

	public class InsertBuilder<T> : IEntityQueryBuilder<T>, IEntityInsertQueryBuilder<T>
		where T : class
	{
		private readonly Schema.Schema _schema;
		private readonly EntityModel<T> _entityModel;
		private readonly EntityExpressionConverter<T> _expressionConverter;

		public IEntityFieldAssignmentBuilder<T> Assignments { get; set; }
		IFieldAssignmentBuilder IFieldAssignmentQueryBuilder.Assignments
		{
			get => Assignments;
			set => Assignments = (IEntityFieldAssignmentBuilder<T>)value;
		}

		public InsertBuilder(Schema.Schema schema, EntityModel<T> entityModel)
		{
			_schema = schema;
			_entityModel = entityModel;
			_expressionConverter = new EntityExpressionConverter<T>(_schema);

			Assignments = new DefaultEntityFieldAssignmentBuilder<T>(schema, _expressionConverter);
		}

		public QueryExpression BuildQuery()
		{
			throw new NotImplementedException();
		}

		public static InsertBuilder<T> Create(Schema.Schema schema, T entity)
		{
			var entityModel = schema.GetEntityModel<T>();
			if (entityModel == null)
				throw new InvalidOperationException($"Entity type `{typeof(T).FullName}` is not present in the schema.");

			var builder = new InsertBuilder<T>(schema, entityModel);
			builder.Assignments.SetAll(entity);
			return builder;
		}

		public static InsertBuilder<T> Create<TView>(Schema.Schema schema, TView entityView)
			where TView : class
		{
			var entityModel = schema.GetEntityModel<T>();
			if (entityModel == null)
				throw new InvalidOperationException($"Entity type `{typeof(T).FullName}` is not present in the schema.");

			var builder = new InsertBuilder<T>(schema, entityModel);
			builder.Assignments.SetAll(entityView);
			return builder;
		}
	}
}
