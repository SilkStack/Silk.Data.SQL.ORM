using System;
using System.Collections.Generic;
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
	}

	public class DefaultEntityConditionBuilder<T> : DefaultConditionBuilder, IEntityConditionBuilder<T>
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

		public Schema.Schema Schema => EntitySchema.Schema;
		public EntitySchema<T> EntitySchema { get; }

		public DefaultEntityConditionBuilder(EntitySchema<T> entitySchema, EntityExpressionConverter<T> expressionConverter = null)
		{
			EntitySchema = entitySchema;
			_expressionConverter = expressionConverter;
		}

		public DefaultEntityConditionBuilder(Schema.Schema schema, EntityExpressionConverter<T> expressionConverter = null)
		{
			EntitySchema = schema.GetEntitySchema<T>();
			_expressionConverter = expressionConverter;
		}

		public DefaultEntityConditionBuilder(Schema.Schema schema, EntitySchemaDefinition<T> entitySchemaDefinition,
			EntityExpressionConverter<T> expressionConverter = null)
		{
			EntitySchema = schema.GetEntitySchema(entitySchemaDefinition);
			_expressionConverter = expressionConverter;
		}

		public void AndAlso(Expression<Func<T, bool>> expression)
			=> AndAlso(ExpressionConverter.Convert(expression));

		public void OrElse(Expression<Func<T, bool>> expression)
			=> OrElse(ExpressionConverter.Convert(expression));
	}
}
