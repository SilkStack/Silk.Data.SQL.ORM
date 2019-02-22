﻿using Silk.Data.Modelling.Mapping;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

		public Schema.Schema Schema { get; }
		public EntityModel<T> EntityModel { get; }

		public DefaultEntityFieldAssignmentBuilder(
			Schema.Schema schema,
			EntityModel<T> entityModel,
			EntityExpressionConverter<T> expressionConverter = null)
		{
			Schema = schema;
			EntityModel = entityModel;
			_expressionConverter = expressionConverter;
		}

		public DefaultEntityFieldAssignmentBuilder(Schema.Schema schema,
			EntityExpressionConverter<T> expressionConverter = null)
			: this(schema, schema.GetEntityModel<T>(), expressionConverter)
		{
		}

		public void Set(EntityField<T> schemaField, T entity)
		{
			var transcriber = EntityModel.GetModelTranscriber<T>();
			var fieldWriter = transcriber.FieldWriters.FirstOrDefault(
				q => q.To == schemaField
				);
			if (fieldWriter == null)
				throw new InvalidOperationException("Specified field has no binding to the entity model.");
			fieldWriter.Write(entity, this);
		}

		public void Set<TValue>(EntityField<T> schemaField, TValue value)
		{
			var valueExpression = ORMQueryExpressions.Value(value);
			AddFieldAssignment(
				QueryExpression.Column(schemaField.Column.Name),
				valueExpression
				);
		}

		public void Set<TValue>(EntityField<T> schemaField, Expression<Func<T, TValue>> valueExpression)
		{
			var valueExpressionResult = ExpressionConverter.Convert(valueExpression);
			AddFieldAssignment(
				QueryExpression.Column(schemaField.Column.Name),
				valueExpressionResult.QueryExpression
				);
		}

		public void Set(EntityField<T> schemaField, IQueryBuilder subQuery)
		{
			AddFieldAssignment(
				QueryExpression.Column(schemaField.Column.Name),
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

		public void SetAll(T entity)
		{
			var graphReader = new ObjectGraphReaderWriter<T>(entity);
			var transcriber = EntityModel.GetModelTranscriber<T>();
			foreach (var fieldWriter in transcriber.FieldWriters)
			{
				if (fieldWriter.To.IsPrimaryKey && fieldWriter.To.IsSeverGenerated)
					continue;

				fieldWriter.Write(graphReader, this);
			}
		}

		public void SetAll<TView>(TView entityView) where TView : class
		{
			var graphReader = new ObjectGraphReaderWriter<TView>(entityView);
			var transcriber = EntityModel.GetModelTranscriber<TView>();
			foreach (var fieldWriter in transcriber.FieldWriters)
			{
				if (fieldWriter.To.IsPrimaryKey && fieldWriter.To.IsSeverGenerated)
					continue;

				fieldWriter.Write(graphReader, this);
			}
		}
	}
}
