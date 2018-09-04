using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
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
				throw new Exception("Condition requires a JOIN which UPDATE doesn't support, consider using a sub-query instead.");
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
				throw new Exception("Condition requires a JOIN which UPDATE doesn't support, consider using a sub-query instead.");
			OrWhere(condition.QueryExpression);
		}

		public void Set(FieldAssignment fieldValuePair)
		{
			_fieldAssignments.Add(fieldValuePair);
		}

		public void Set<TValue>(EntityField<TValue, T> entityField, TValue value)
		{
			Set(new FieldValueAssignment<TValue>(entityField, new StaticValueReader<TValue>(value)));
		}

		public void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, TProperty value)
		{
		}

		public void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, Expression valueExpression)
		{
		}

		public void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, IQueryBuilder subQuery)
		{
		}

		public void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, QueryExpression queryExpression)
		{
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
