using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Queries.Expressions;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Queries
{
	public class DefaultFieldAssignmentBuilder : IFieldAssignmentBuilder
	{
		private readonly List<FieldAssignment> _fieldAssignments = new List<FieldAssignment>();

		protected void AddFieldAssignment(ColumnExpression columnExpression, QueryExpression valueExpression)
		{
			_fieldAssignments.Add(new FieldAssignment(
					columnExpression, valueExpression
					));
		}

		public void Set(ColumnExpression columnExpression, QueryExpression valueExpression)
		{
			AddFieldAssignment(
				columnExpression,
				valueExpression
				);
		}

		public void Set(ColumnExpression columnExpression, object value)
		{
			var valueExpression = ORMQueryExpressions.Value(value);
			AddFieldAssignment(
				columnExpression,
				valueExpression
				);
		}

		private IEnumerable<AssignColumnExpression> GetAssignColumnExpressions()
		{
			foreach (var fieldAssignment in _fieldAssignments)
			{
				yield return QueryExpression.Assign(fieldAssignment.Column.ColumnName, fieldAssignment.ValueExpression);
			}
		}

		public AssignColumnExpression[] Build()
			=> GetAssignColumnExpressions().ToArray();

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

	public class DefaultEntityFieldAssignmentBuilder<T> : DefaultFieldAssignmentBuilder, IEntityFieldAssignmentBuilder<T>
		where T : class
	{
		private ObjectReadWriter _entityReadWriter;
		public ObjectReadWriter EntityReadWriter
		{
			get
			{
				if (_entityReadWriter == null)
					_entityReadWriter = new ObjectReadWriter(null, TypeModel.GetModelOf<T>(), typeof(T));
				return _entityReadWriter;
			}
		}

		private EntityExpressionConverter<T> _expressionConverter;
		public EntityExpressionConverter<T> ExpressionConverter
		{
			get
			{
				if (_expressionConverter == null)
					_expressionConverter = new EntityExpressionConverter<T>(Schema);
				return _expressionConverter;
			}
		}

		public Schema.Schema Schema => EntitySchema.Schema;
		public EntitySchema<T> EntitySchema { get; }

		public DefaultEntityFieldAssignmentBuilder(EntitySchema<T> entitySchema, EntityExpressionConverter<T> expressionConverter = null)
		{
			EntitySchema = entitySchema;
			_expressionConverter = expressionConverter;
		}

		public DefaultEntityFieldAssignmentBuilder(Schema.Schema schema, EntityExpressionConverter<T> expressionConverter = null)
		{
			EntitySchema = schema.GetEntitySchema<T>();
			_expressionConverter = expressionConverter;
		}

		public DefaultEntityFieldAssignmentBuilder(Schema.Schema schema, EntitySchemaDefinition<T> entitySchemaDefinition,
			EntityExpressionConverter<T> expressionConverter = null)
		{
			EntitySchema = schema.GetEntitySchema(entitySchemaDefinition);
			_expressionConverter = expressionConverter;
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
	}
}
