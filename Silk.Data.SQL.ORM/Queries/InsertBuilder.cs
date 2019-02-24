using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class InsertBuilder<T> : IEntityInsertQueryBuilder<T>
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

		public InsertBuilder(Schema.Schema schema) : this(schema, schema.GetEntityModel<T>())
		{
		}

		public QueryExpression BuildQuery()
		{
			var row = Assignments.Build();
			var columnNames = row?.Select(q => q.Column.ColumnName).ToArray() ?? new string[0];
			var values = row?.Select(q => q.Expression).ToArray() ?? new QueryExpression[0];

			return QueryExpression.Insert(
				_entityModel.Table.TableName,
				columnNames,
				values
				);
		}

		public static InsertBuilder<T> Create(Schema.Schema schema, T entity)
		{
			var entityModel = schema.GetEntityModel<T>();
			if (entityModel == null)
				ExceptionHelper.ThrowNotPresentInSchema<T>();

			return Create(schema, entityModel, entity);
		}

		public static InsertBuilder<T> Create(Schema.Schema schema, EntityModel<T> entityModel, T entity)
		{
			var builder = new InsertBuilder<T>(schema, entityModel);
			builder.Assignments.SetAll(entity);
			return builder;
		}

		public static InsertBuilder<T> Create<TView>(Schema.Schema schema, TView entityView)
			where TView : class
		{
			var entityModel = schema.GetEntityModel<T>();
			if (entityModel == null)
				ExceptionHelper.ThrowNotPresentInSchema<T>();

			return Create(schema, entityModel, entityView);
		}

		public static InsertBuilder<T> Create<TView>(Schema.Schema schema, EntityModel<T> entityModel, TView entityView)
			where TView : class
		{
			var builder = new InsertBuilder<T>(schema, entityModel);
			builder.Assignments.SetAll(entityView);
			return builder;
		}
	}
}
