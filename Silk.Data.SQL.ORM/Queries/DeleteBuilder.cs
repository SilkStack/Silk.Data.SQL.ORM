using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public class DeleteBuilder : IQueryBuilder
	{
		protected TableExpression Source { get; set; }
		protected QueryExpression Where { get; private set; }
		protected QueryExpression Limit { get; private set; }

		public QueryExpression BuildQuery()
		{
			return QueryExpression.Delete(
				Source, Where, Limit
				);
		}

		public void AndWhere(QueryExpression queryExpression)
		{
			Where = QueryExpression.CombineConditions(Where, ConditionType.AndAlso, queryExpression);
		}

		public void OrWhere(QueryExpression queryExpression)
		{
			Where = QueryExpression.CombineConditions(Where, ConditionType.OrElse, queryExpression);
		}
	}

	public class DeleteBuilder<T> : DeleteBuilder
	{
	}

	public class EntityDeleteBuilder<T> : DeleteBuilder<T>
		where T : class
	{
		private ExpressionConverter<T> _expressionConverter;
		private ExpressionConverter<T> ExpressionConverter
		{
			get
			{
				if (_expressionConverter == null)
					_expressionConverter = new ExpressionConverter<T>(Schema);
				return _expressionConverter;
			}
		}

		public Schema.Schema Schema { get; }
		public EntitySchema<T> EntitySchema { get; }

		public EntityDeleteBuilder(Schema.Schema schema)
		{
			Schema = schema;
			EntitySchema = schema.GetEntitySchema<T>();
			if (EntitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			Source = QueryExpression.Table(EntitySchema.EntityTable.TableName);
		}

		public void AndWhere(Expression<Func<T, bool>> expression)
		{
			var condition = ExpressionConverter.Convert(expression);
			if (condition.RequiredJoins != null && condition.RequiredJoins.Length > 0)
				throw new Exception("Condition requires a JOIN which DELETE doesn't support, consider using a sub-query instead.");
			AndWhere(condition.QueryExpression);
		}

		public void OrWhere(Expression<Func<T, bool>> expression)
		{
			var condition = ExpressionConverter.Convert(expression);
			if (condition.RequiredJoins != null && condition.RequiredJoins.Length > 0)
				throw new Exception("Condition requires a JOIN which DELETE doesn't support, consider using a sub-query instead.");
			OrWhere(condition.QueryExpression);
		}
	}
}
