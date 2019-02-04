using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Queries
{
	public class ConditionBuilder : IConditionBuilder
	{
		private QueryExpression _expression;
		private List<EntityFieldJoin> _requiredJoins
			= new List<EntityFieldJoin>();

		private void AddJoins(EntityFieldJoin[] joins)
		{
			if (joins == null || joins.Length < 1)
				return;
			foreach (var join in joins)
			{
				AddJoins(join);
			}
		}

		private void AddJoins(EntityFieldJoin join)
		{
			if (join == null || _requiredJoins.Contains(join))
				return;
			_requiredJoins.Add(join);
			AddJoins(join.DependencyJoins);
		}

		public ExpressionResult Build()
		{
			return new ExpressionResult(
				_expression,
				_requiredJoins.ToArray()
				);
		}

		public void AndAlso(QueryExpression queryExpression)
		{
			_expression = QueryExpression.CombineConditions(
				_expression,
				ConditionType.AndAlso,
				queryExpression
				);
		}

		public void OrElse(QueryExpression queryExpression)
		{
			_expression = QueryExpression.CombineConditions(
				_expression,
				ConditionType.OrElse,
				queryExpression
				);
		}

		public void AndAlso(ExpressionResult expressionResult)
		{
			AndAlso(expressionResult.QueryExpression);
			AddJoins(expressionResult.RequiredJoins);
		}

		public void OrElse(ExpressionResult expressionResult)
		{
			OrElse(expressionResult.QueryExpression);
			AddJoins(expressionResult.RequiredJoins);
		}
	}

	public class EntityConditionBuilder<T> : ConditionBuilder, IEntityConditionBuilder<T>
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

		public EntityConditionBuilder(EntitySchema<T> entitySchema, EntityExpressionConverter<T> expressionConverter = null)
		{
			EntitySchema = entitySchema;
			_expressionConverter = expressionConverter;
		}

		public EntityConditionBuilder(Schema.Schema schema, EntityExpressionConverter<T> expressionConverter = null)
		{
			EntitySchema = schema.GetEntitySchema<T>();
			_expressionConverter = expressionConverter;
		}

		public EntityConditionBuilder(Schema.Schema schema, EntitySchemaDefinition<T> entitySchemaDefinition,
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
