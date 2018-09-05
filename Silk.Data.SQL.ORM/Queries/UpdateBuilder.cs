using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Queries
{
	public class UpdateBuilder<T> : QueryBuilderBase<T>
		where T : class
	{
		private readonly List<FieldAssignment> _fieldAssignments = new List<FieldAssignment>();
		private QueryExpression _where;

		public UpdateBuilder(Schema.Schema schema) : base(schema) { }

		public UpdateBuilder(EntitySchema<T> schema) : base(schema) { }

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
					column,
					comparisonOperator,
					valueExpression
					));
			}
		}

		public void AndWhere(Expression<Func<T, bool>> expression)
		{
			var condition = ExpressionConverter.Convert(expression);
			if (condition.RequiredJoins != null && condition.RequiredJoins.Length > 0)
				throw new Exception("Condition requires a JOIN which UPDATE doesn't support, consider using a sub-query instead.");
			AndWhere(condition.QueryExpression);
		}

		public void OrWhere(FieldAssignment field, ComparisonOperator comparisonOperator)
		{
			foreach (var (column, valueExpression) in field.GetColumnExpressionPairs())
			{
				OrWhere(QueryExpression.Compare(
					column,
					comparisonOperator,
					valueExpression
					));
			}
		}

		public void OrWhere(Expression<Func<T, bool>> expression)
		{
			var condition = ExpressionConverter.Convert(expression);
			if (condition.RequiredJoins != null && condition.RequiredJoins.Length > 0)
				throw new Exception("Condition requires a JOIN which UPDATE doesn't support, consider using a sub-query instead.");
			OrWhere(condition.QueryExpression);
		}

		public void Set(FieldAssignment fieldValuePair)
		{
			_fieldAssignments.Add(fieldValuePair);
		}

		public void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, TProperty value)
		{
			Set(fieldSelector, QueryExpression.Value(value));
		}

		public void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, Expression<Func<T, TProperty>> valueExpression)
		{
			var selectorExpressionResult = ExpressionConverter.Convert(fieldSelector);
			var valueExpressionResult = ExpressionConverter.Convert(valueExpression);

			if (selectorExpressionResult.QueryExpression is ColumnExpression columnExpression)
			{
				_fieldAssignments.Add(new FieldExpressionAssignment<TProperty>(
					columnExpression, valueExpressionResult.QueryExpression
					));
				return;
			}
			throw new ArgumentException("Field selector doesn't specify a valid column.", nameof(fieldSelector));
		}

		public void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, Expression valueExpression)
		{
			var selectorExpressionResult = ExpressionConverter.Convert(fieldSelector);
			var valueExpressionResult = ExpressionConverter.Convert(valueExpression);

			if (selectorExpressionResult.QueryExpression is ColumnExpression columnExpression)
			{
				_fieldAssignments.Add(new FieldExpressionAssignment<TProperty>(
					columnExpression, valueExpressionResult.QueryExpression
					));
				return;
			}
			throw new ArgumentException("Field selector doesn't specify a valid column.", nameof(fieldSelector));
		}

		public void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, IQueryBuilder subQuery)
		{
			Set(fieldSelector, subQuery.BuildQuery());
		}

		public void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, QueryExpression queryExpression)
		{
			var selectorExpressionResult = ExpressionConverter.Convert(fieldSelector);

			if (selectorExpressionResult.QueryExpression is ColumnExpression columnExpression)
			{
				_fieldAssignments.Add(new FieldExpressionAssignment<TProperty>(
					columnExpression, queryExpression
					));
				return;
			}
			throw new ArgumentException("Field selector doesn't specify a valid column.", nameof(fieldSelector));
		}

		public override QueryExpression BuildQuery()
		{
			return QueryExpression.Update(
				Source,
				where: _where,
				assignments: GetAssignColumnExpressions().ToArray()
				);
		}

		private IEnumerable<AssignColumnExpression> GetAssignColumnExpressions()
		{
			foreach (var fieldAssignment in _fieldAssignments)
			{
				foreach (var (column, valueExpression) in fieldAssignment.GetColumnExpressionPairs())
				{
					yield return QueryExpression.Assign(column.ColumnName, valueExpression);
				}
			}
		}
	}
}
