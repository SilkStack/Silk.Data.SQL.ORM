using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries.Expressions;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Queries
{
	public class EntityUpdateBuilder<T> : QueryBuilderBase<T>
		where T : class
	{
		private readonly List<FieldAssignment> _fieldAssignments = new List<FieldAssignment>();
		private QueryExpression _where;

		public EntityUpdateBuilder(Schema.Schema schema, ObjectReadWriter entityReadWriter = null) : base(schema, entityReadWriter) { }

		public EntityUpdateBuilder(EntitySchema<T> schema, ObjectReadWriter entityReadWriter = null) : base(schema, entityReadWriter) { }

		public void AndWhere(QueryExpression queryExpression)
		{
			_where = QueryExpression.CombineConditions(_where, ConditionType.AndAlso, queryExpression);
		}

		public void OrWhere(QueryExpression queryExpression)
		{
			_where = QueryExpression.CombineConditions(_where, ConditionType.OrElse, queryExpression);
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
			var valueExpression = ORMQueryExpressions.Value(value);
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
				throw new Exception("Condition requires a JOIN which UPDATE doesn't support, consider using a sub-query instead.");
			AndWhere(condition.QueryExpression);
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
			var valueExpression = ORMQueryExpressions.Value(value);
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
				throw new Exception("Condition requires a JOIN which UPDATE doesn't support, consider using a sub-query instead.");
			OrWhere(condition.QueryExpression);
		}

		private void AddFieldAssignment(ColumnExpression columnExpression, QueryExpression valueExpression)
		{
			_fieldAssignments.Add(new FieldAssignment(
					columnExpression, valueExpression
					));
		}

		public void Set(ISchemaField<T> schemaField, T entity)
		{
			var fieldOperations = Schema.GetFieldOperations(schemaField);
			var valueExpression = fieldOperations.Expressions.Value(entity, EntityReadWriter);
			AddFieldAssignment(
				QueryExpression.Column(schemaField.Column.ColumnName),
				valueExpression
				);
		}

		public void Set<TValue>(ISchemaField<T> schemaField, TValue value)
		{
			var valueExpression = ORMQueryExpressions.Value(value);
			AddFieldAssignment(
				QueryExpression.Column(schemaField.Column.ColumnName),
				valueExpression
				);
		}

		public void Set<TValue>(ISchemaField<T> schemaField, Expression<Func<T, TValue>> valueExpression)
		{
			var valueExpressionResult = ExpressionConverter.Convert(valueExpression);
			AddFieldAssignment(
				QueryExpression.Column(schemaField.Column.ColumnName),
				valueExpressionResult.QueryExpression
				);
		}

		public void Set<TValue>(ISchemaField<T> schemaField, IQueryBuilder subQuery)
		{
			AddFieldAssignment(
				QueryExpression.Column(schemaField.Column.ColumnName),
				subQuery.BuildQuery()
				);
		}

		public void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, TProperty value)
		{
			Set(fieldSelector, ORMQueryExpressions.Value(value));
		}

		public void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, Expression<Func<T, TProperty>> valueExpression)
		{
			var selectorExpressionResult = ExpressionConverter.Convert(fieldSelector);
			var valueExpressionResult = ExpressionConverter.Convert(valueExpression);

			if (selectorExpressionResult.QueryExpression is ColumnExpression columnExpression)
			{
				AddFieldAssignment(
					columnExpression, valueExpressionResult.QueryExpression
				);
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
				AddFieldAssignment(
					columnExpression, valueExpressionResult.QueryExpression
				);
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
				AddFieldAssignment(
					columnExpression, queryExpression
				);
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
				yield return QueryExpression.Assign(fieldAssignment.Column.ColumnName, fieldAssignment.ValueExpression);
			}
		}

		private class FieldAssignment
		{
			public ColumnExpression Column { get; }
			public QueryExpression ValueExpression { get; }

			public FieldAssignment(ColumnExpression column, QueryExpression valueExpression)
			{
				Column = column;
				ValueExpression = valueExpression;
			}
		}
	}
}
