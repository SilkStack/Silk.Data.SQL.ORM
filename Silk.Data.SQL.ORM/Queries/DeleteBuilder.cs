using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public class DeleteBuilder<T> : IQueryBuilder
		where T : class
	{
		private readonly TableExpression _source;
		private QueryExpression _where;

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

		public DeleteBuilder(Schema.Schema schema)
		{
			Schema = schema;
			EntitySchema = schema.GetEntitySchema<T>();
			if (EntitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			_source = QueryExpression.Table(EntitySchema.EntityTable.TableName);
		}

		public DeleteBuilder(EntitySchema<T> schema)
		{
			EntitySchema = schema;
			Schema = schema.Schema;
			_source = QueryExpression.Table(EntitySchema.EntityTable.TableName);
		}

		public void AndWhere(QueryExpression queryExpression)
		{
			_where = QueryExpression.CombineConditions(_where, ConditionType.AndAlso, queryExpression);
		}

		public void OrWhere(QueryExpression queryExpression)
		{
			_where = QueryExpression.CombineConditions(_where, ConditionType.OrElse, queryExpression);
		}

		public void AndWhere(FieldAssignment field, ComparisonOperator comparisonOperator)
		{
			foreach (var (column, valueExpression) in field.GetColumnExpressionPairs())
			{
				AndWhere(QueryExpression.Compare(
					QueryExpression.Column(column.ColumnName, _source),
					comparisonOperator,
					valueExpression
					));
			}
		}

		public void AndWhere(Expression<Func<T, bool>> expression)
		{
			var condition = ExpressionConverter.Convert(expression);
			if (condition.RequiredJoins != null && condition.RequiredJoins.Length > 0)
				throw new Exception("Condition requires a JOIN which DELETE doesn't support, consider using a sub-query instead.");
			AndWhere(condition.QueryExpression);
		}

		public void OrWhere(FieldAssignment field, ComparisonOperator comparisonOperator)
		{
			foreach (var (column, valueExpression) in field.GetColumnExpressionPairs())
			{
				OrWhere(QueryExpression.Compare(
					QueryExpression.Column(column.ColumnName, _source),
					comparisonOperator,
					valueExpression
					));
			}
		}

		public void OrWhere(Expression<Func<T, bool>> expression)
		{
			var condition = ExpressionConverter.Convert(expression);
			if (condition.RequiredJoins != null && condition.RequiredJoins.Length > 0)
				throw new Exception("Condition requires a JOIN which DELETE doesn't support, consider using a sub-query instead.");
			OrWhere(condition.QueryExpression);
		}

		public QueryExpression BuildQuery()
		{
			return QueryExpression.Delete(
				_source, _where
				);
		}
	}
}
