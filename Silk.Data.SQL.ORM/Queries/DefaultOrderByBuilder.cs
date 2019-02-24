using System;
using System.Collections.Generic;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Queries
{
	public class DefaultOrderByBuilder : IOrderByBuilder
	{
		private readonly List<ExpressionResult> _orderBy = new List<ExpressionResult>();

		public ExpressionResult[] Build()
			=> _orderBy.ToArray();

		public void Ascending(QueryExpression queryExpression)
			=> _orderBy.Add(new ExpressionResult(queryExpression));

		public void Ascending(ExpressionResult expressionResult)
			=> _orderBy.Add(expressionResult);

		public void Descending(QueryExpression queryExpression)
			=> _orderBy.Add(new ExpressionResult(QueryExpression.Descending(queryExpression)));

		public void Descending(ExpressionResult expressionResult)
			=> _orderBy.Add(
				new ExpressionResult(QueryExpression.Descending(expressionResult.QueryExpression),
					expressionResult.RequiredJoins));
	}

	public class DefaultEntityOrderByBuilder<T> : DefaultOrderByBuilder, IEntityOrderByBuilder<T>
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
		public EntityModel<T> EntitySchema { get; }

		public DefaultEntityOrderByBuilder(Schema.Schema schema, EntityModel<T> entitySchema, EntityExpressionConverter<T> expressionConverter = null)
		{
			Schema = schema;
			EntitySchema = entitySchema;
			_expressionConverter = expressionConverter;
		}

		public DefaultEntityOrderByBuilder(Schema.Schema schema, EntityExpressionConverter<T> expressionConverter = null) :
			this(schema, schema.GetEntityModel<T>(), expressionConverter)
		{
		}

		public void Ascending<TProperty>(System.Linq.Expressions.Expression<Func<T, TProperty>> expression)
			=> Ascending(ExpressionConverter.Convert(expression));

		public void Descending<TProperty>(System.Linq.Expressions.Expression<Func<T, TProperty>> expression)
			=> Descending(ExpressionConverter.Convert(expression));
	}
}
