using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public class DeleteBuilder<T> : QueryBuilderBase<T>
		where T : class
	{
		private QueryExpression _where;

		public DeleteBuilder(Schema.Schema schema) : base(schema) { }

		public DeleteBuilder(EntitySchema<T> schema) : base(schema) { }

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
					QueryExpression.Column(column.ColumnName, Source),
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
					QueryExpression.Column(column.ColumnName, Source),
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

		public override QueryExpression BuildQuery()
		{
			return QueryExpression.Delete(
				Source, _where
				);
		}
	}

	public class DeleteBuilder<TLeft, TRight> : QueryBuilderBase<TLeft, TRight>
		where TLeft : class
		where TRight : class
	{
		private QueryExpression _where;

		public DeleteBuilder(Schema.Schema schema, string relationshipName) : base(schema, relationshipName) { }

		public DeleteBuilder(Relationship<TLeft, TRight> relationship) : base(relationship) { }

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
					QueryExpression.Column(column.ColumnName, Source),
					comparisonOperator,
					valueExpression
					));
			}
		}

		//public void AndWhere(Expression<Func<T, bool>> expression)
		//{
		//	var condition = ExpressionConverter.Convert(expression);
		//	if (condition.RequiredJoins != null && condition.RequiredJoins.Length > 0)
		//		throw new Exception("Condition requires a JOIN which DELETE doesn't support, consider using a sub-query instead.");
		//	AndWhere(condition.QueryExpression);
		//}

		public void OrWhere(FieldAssignment field, ComparisonOperator comparisonOperator)
		{
			foreach (var (column, valueExpression) in field.GetColumnExpressionPairs())
			{
				OrWhere(QueryExpression.Compare(
					QueryExpression.Column(column.ColumnName, Source),
					comparisonOperator,
					valueExpression
					));
			}
		}

		//public void OrWhere(Expression<Func<T, bool>> expression)
		//{
		//	var condition = ExpressionConverter.Convert(expression);
		//	if (condition.RequiredJoins != null && condition.RequiredJoins.Length > 0)
		//		throw new Exception("Condition requires a JOIN which DELETE doesn't support, consider using a sub-query instead.");
		//	OrWhere(condition.QueryExpression);
		//}

		public override QueryExpression BuildQuery()
		{
			return QueryExpression.Delete(
				Source, _where
				);
		}
	}
}
