using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public class EntityDeleteBuilder<T> : QueryBuilderBase<T>
		where T : class
	{
		private QueryExpression _where;

		public EntityDeleteBuilder(Schema.Schema schema, ObjectReadWriter entityReadWriter = null) : base(schema, entityReadWriter) { }

		public EntityDeleteBuilder(EntitySchema<T> schema, ObjectReadWriter entityReadWriter = null) : base(schema, entityReadWriter) { }

		public void AndWhere(QueryExpression queryExpression)
		{
			_where = QueryExpression.CombineConditions(_where, ConditionType.AndAlso, queryExpression);
		}

		public void AndWhere(ISchemaField<T> schemaField, ComparisonOperator @operator, T entity)
		{
			var fieldOperations = Schema.GetFieldOperations(schemaField);
			var valueExpression = fieldOperations.Expressions.Value(entity, EntityReadWriter);
			AndWhere(QueryExpression.Compare(
				QueryExpression.Column(schemaField.Column.ColumnName),
				@operator,
				valueExpression
				));
		}

		public void AndWhere<TValue>(ISchemaField<T> schemaField, ComparisonOperator @operator, TValue value)
		{
			var valueExpression = QueryExpression.Value(value);
			AndWhere(QueryExpression.Compare(
				QueryExpression.Column(schemaField.Column.ColumnName),
				@operator,
				valueExpression
				));
		}

		public void AndWhere(ISchemaField<T> schemaField, ComparisonOperator @operator, Expression<Func<T, bool>> valueExpression)
		{
			var valueExpressionResult = ExpressionConverter.Convert(valueExpression);
			AndWhere(QueryExpression.Compare(
				QueryExpression.Column(schemaField.Column.ColumnName),
				@operator,
				valueExpressionResult.QueryExpression
				));
		}

		public void AndWhere<TValue>(ISchemaField<T> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
		{
			AndWhere(QueryExpression.Compare(
				QueryExpression.Column(schemaField.Column.ColumnName),
				@operator,
				subQuery.BuildQuery()
				));
		}

		public void AndWhere(Expression<Func<T, bool>> expression)
		{
			var condition = ExpressionConverter.Convert(expression);
			if (condition.RequiredJoins != null && condition.RequiredJoins.Length > 0)
				throw new Exception("Condition requires a JOIN which DELETE doesn't support, consider using a sub-query instead.");
			AndWhere(condition.QueryExpression);
		}

		public void OrWhere(QueryExpression queryExpression)
		{
			_where = QueryExpression.CombineConditions(_where, ConditionType.OrElse, queryExpression);
		}

		public void OrWhere(ISchemaField<T> schemaField, ComparisonOperator @operator, T entity)
		{
			var fieldOperations = Schema.GetFieldOperations(schemaField);
			var valueExpression = fieldOperations.Expressions.Value(entity, EntityReadWriter);
			OrWhere(QueryExpression.Compare(
				QueryExpression.Column(schemaField.Column.ColumnName),
				@operator,
				valueExpression
				));
		}

		public void OrWhere<TValue>(ISchemaField<T> schemaField, ComparisonOperator @operator, TValue value)
		{
			var valueExpression = QueryExpression.Value(value);
			OrWhere(QueryExpression.Compare(
				QueryExpression.Column(schemaField.Column.ColumnName),
				@operator,
				valueExpression
				));
		}

		public void OrWhere(ISchemaField<T> schemaField, ComparisonOperator @operator, Expression<Func<T, bool>> valueExpression)
		{
			var valueExpressionResult = ExpressionConverter.Convert(valueExpression);
			OrWhere(QueryExpression.Compare(
				QueryExpression.Column(schemaField.Column.ColumnName),
				@operator,
				valueExpressionResult.QueryExpression
				));
		}

		public void OrWhere<TValue>(ISchemaField<T> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
		{
			OrWhere(QueryExpression.Compare(
				QueryExpression.Column(schemaField.Column.ColumnName),
				@operator,
				subQuery.BuildQuery()
				));
		}

		public void OrWhere(Expression<Func<T, bool>> expression)
		{
			var condition = ExpressionConverter.Convert(expression);
			if (condition.RequiredJoins != null && condition.RequiredJoins.Length > 0)
				throw new Exception("Condition requires a JOIN which DELETE doesn't support, consider using a sub-query instead.");
			OrWhere(condition.QueryExpression);
		}

		public override QueryExpression BuildQuery()
		{
			return QueryExpression.Delete(
				Source, _where
				);
		}
	}
}
