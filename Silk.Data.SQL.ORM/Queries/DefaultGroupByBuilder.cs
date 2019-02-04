using System;
using System.Collections.Generic;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Queries
{
	public class DefaultGroupByBuilder : IGroupByBuilder
	{
		private readonly List<ExpressionResult> _groupBy = new List<ExpressionResult>();

		public ExpressionResult[] Build()
			=> _groupBy.ToArray();

		public void GroupBy(QueryExpression queryExpression)
			=> _groupBy.Add(new ExpressionResult(queryExpression));

		public void GroupBy(ExpressionResult expressionResult)
			=> _groupBy.Add(expressionResult);
	}

	public class DefaultEntityGroupByBuilder<T> : DefaultGroupByBuilder, IEntityGroupByBuilder<T>
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

		public DefaultEntityGroupByBuilder(EntitySchema<T> entitySchema, EntityExpressionConverter<T> expressionConverter = null)
		{
			EntitySchema = entitySchema;
			_expressionConverter = expressionConverter;
		}

		public DefaultEntityGroupByBuilder(Schema.Schema schema, EntityExpressionConverter<T> expressionConverter = null)
		{
			EntitySchema = schema.GetEntitySchema<T>();
			_expressionConverter = expressionConverter;
		}

		public DefaultEntityGroupByBuilder(Schema.Schema schema, EntitySchemaDefinition<T> entitySchemaDefinition,
			EntityExpressionConverter<T> expressionConverter = null)
		{
			EntitySchema = schema.GetEntitySchema(entitySchemaDefinition);
			_expressionConverter = expressionConverter;
		}

		public void GroupBy<TProperty>(System.Linq.Expressions.Expression<Func<T, TProperty>> expression)
			=> GroupBy(ExpressionConverter.Convert(expression));
	}
}
