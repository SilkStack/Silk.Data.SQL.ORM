using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Queries
{
	public class UpdateBuilder<T>
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

		private readonly List<FieldAssignment> _fieldAssignments = new List<FieldAssignment>();
		private readonly TableExpression _source;
		private QueryExpression _where;

		public UpdateBuilder(Schema.Schema schema)
		{
			Schema = schema;
			EntitySchema = schema.GetEntitySchema<T>();
			if (EntitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			_source = QueryExpression.Table(EntitySchema.EntityTable.TableName);
		}

		public UpdateBuilder(EntitySchema<T> schema)
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
					QueryExpression.Column(column.ColumnName),
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
					QueryExpression.Column(column.ColumnName),
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

		public QueryExpression BuildQuery()
		{
			return QueryExpression.Update(
				_source,
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
