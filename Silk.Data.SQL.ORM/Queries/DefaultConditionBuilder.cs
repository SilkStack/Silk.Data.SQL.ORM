﻿using System;
using System.Linq;
using System.Linq.Expressions;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Queries
{
	public class DefaultConditionBuilder : IConditionBuilder
	{
		private QueryExpression _expression;
		private readonly JoinCollection _requiredJoins = new JoinCollection();

		public ExpressionResult Build()
		{
			return new ExpressionResult(
				_expression,
				_requiredJoins.ToArray()
				);
		}

		public void AndAlso(QueryExpression queryExpression)
			=> _expression = QueryExpression.CombineConditions(
				_expression,
				ConditionType.AndAlso,
				queryExpression
				);

		public void OrElse(QueryExpression queryExpression)
			=> _expression = QueryExpression.CombineConditions(
				_expression,
				ConditionType.OrElse,
				queryExpression
				);

		public void AndAlso(ExpressionResult expressionResult)
		{
			AndAlso(expressionResult.QueryExpression);
			_requiredJoins.AddJoins(expressionResult.RequiredJoins);
		}

		public void OrElse(ExpressionResult expressionResult)
		{
			OrElse(expressionResult.QueryExpression);
			_requiredJoins.AddJoins(expressionResult.RequiredJoins);
		}

		protected void AddJoin(IQueryReference queryReference)
		{
			var join = queryReference as Join;
			if (join == null)
				return;
			_requiredJoins.AddJoin(join);
		}
	}

	public class DefaultEntityConditionBuilder<T> : DefaultConditionBuilder, IEntityConditionBuilder<T>
		where T : class
	{
		private EntityExpressionConverter<T> _expressionConverter;
		private readonly IModelTranscriber<T> _modelTranscriber;

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

		public DefaultEntityConditionBuilder(
			Schema.Schema schema, EntityModel<T> entitySchema,
			EntityExpressionConverter<T> expressionConverter = null,
			IModelTranscriber<T> modelTranscriber = null)
		{
			Schema = schema;
			EntityModel = entitySchema;
			_expressionConverter = expressionConverter;
			_modelTranscriber = modelTranscriber ?? entitySchema.GetModelTranscriber<T>();
		}

		public DefaultEntityConditionBuilder(Schema.Schema schema, EntityExpressionConverter<T> expressionConverter = null,
			IModelTranscriber<T> modelTranscriber = null) :
			this(schema, schema.GetEntityModel<T>(), expressionConverter, modelTranscriber)
		{
		}

		private ValueExpression GetValueExpression(EntityField<T> entityField, T entity)
		{
			var helper = _modelTranscriber.ObjectToSchemaHelpers.FirstOrDefault(q => q.To == entityField);
			if (helper == null)
				ExceptionHelper.ThrowEntityFieldNotFound();
			return helper.WriteValueExpression(entity);
		}

		public void AndAlso(Expression<Func<T, bool>> expression)
			=> AndAlso(ExpressionConverter.Convert(expression));

		public void OrElse(Expression<Func<T, bool>> expression)
			=> OrElse(ExpressionConverter.Convert(expression));

		public void AndAlso(EntityField<T> schemaField, ComparisonOperator @operator, T entity)
		{
			AndAlso(QueryExpression.Compare(
				QueryExpression.Column(schemaField.Column.Name),
				@operator,
				GetValueExpression(schemaField, entity)
				));
			AddJoin(schemaField.Source);
		}

		public void AndAlso<TValue>(EntityField<T> schemaField, ComparisonOperator @operator, TValue value)
		{
			var valueExpression = ORMQueryExpressions.Value(value);
			AndAlso(QueryExpression.Compare(
				QueryExpression.Column(schemaField.Column.Name),
				@operator,
				valueExpression
				));
			AddJoin(schemaField.Source);
		}

		public void AndAlso(EntityField<T> schemaField, ComparisonOperator @operator, Expression<Func<T, bool>> valueExpression)
		{
			var valueExpressionResult = ExpressionConverter.Convert(valueExpression);
			AndAlso(QueryExpression.Compare(
				QueryExpression.Column(schemaField.Column.Name),
				@operator,
				valueExpressionResult.QueryExpression
				));
			AddJoin(schemaField.Source);
		}

		public void AndAlso(EntityField<T> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
		{
			AndAlso(QueryExpression.Compare(
				QueryExpression.Column(schemaField.Column.Name),
				@operator,
				subQuery.BuildQuery()
				));
			AddJoin(schemaField.Source);
		}

		public void OrElse(EntityField<T> schemaField, ComparisonOperator @operator, T entity)
		{
			OrElse(QueryExpression.Compare(
				QueryExpression.Column(schemaField.Column.Name),
				@operator,
				GetValueExpression(schemaField, entity)
				));
			AddJoin(schemaField.Source);
		}

		public void OrElse<TValue>(EntityField<T> schemaField, ComparisonOperator @operator, TValue value)
		{
			var valueExpression = ORMQueryExpressions.Value(value);
			OrElse(QueryExpression.Compare(
				QueryExpression.Column(schemaField.Column.Name),
				@operator,
				valueExpression
				));
			AddJoin(schemaField.Source);
		}

		public void OrElse(EntityField<T> schemaField, ComparisonOperator @operator, Expression<Func<T, bool>> valueExpression)
		{
			var valueExpressionResult = ExpressionConverter.Convert(valueExpression);
			OrElse(QueryExpression.Compare(
				QueryExpression.Column(schemaField.Column.Name),
				@operator,
				valueExpressionResult.QueryExpression
				));
			AddJoin(schemaField.Source);
		}

		public void OrElse(EntityField<T> schemaField, ComparisonOperator @operator, IQueryBuilder subQuery)
		{
			OrElse(QueryExpression.Compare(
				QueryExpression.Column(schemaField.Column.Name),
				@operator,
				subQuery.BuildQuery()
				));
			AddJoin(schemaField.Source);
		}
	}
}