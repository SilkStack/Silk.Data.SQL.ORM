﻿using System;
using System.Linq;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Queries
{
	public class DefaultRangeBuilder : IRangeBuilder
	{
		private QueryExpression _limitExpression;
		private QueryExpression _offsetExpression;
		private readonly JoinCollection _requiredJoins = new JoinCollection();

		public ExpressionResult BuildLimit()
			=> new ExpressionResult(_limitExpression, _requiredJoins.ToArray());

		public ExpressionResult BuildOffset()
			=> new ExpressionResult(_offsetExpression, _requiredJoins.ToArray());

		public void Limit(int limit)
			=> _limitExpression = ORMQueryExpressions.Value(limit);

		public void Limit(QueryExpression queryExpression)
			=> _limitExpression = queryExpression;

		public void Limit(ExpressionResult expressionResult)
		{
			_limitExpression = expressionResult.QueryExpression;
			_requiredJoins.AddJoins(expressionResult.RequiredJoins);
		}

		public void Offset(int offset)
			=> _offsetExpression = ORMQueryExpressions.Value(offset);

		public void Offset(QueryExpression queryExpression)
			=> _offsetExpression = queryExpression;

		public void Offset(ExpressionResult expressionResult)
		{
			_offsetExpression = expressionResult.QueryExpression;
			_requiredJoins.AddJoins(expressionResult.RequiredJoins);
		}
	}

	public class DefaultEntityRangeBuilder<T> : DefaultRangeBuilder, IEntityRangeBuilder<T>
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

		public DefaultEntityRangeBuilder(Schema.Schema schema, EntityModel<T> entitySchema, EntityExpressionConverter<T> expressionConverter = null)
		{
			Schema = schema;
			EntitySchema = entitySchema;
			_expressionConverter = expressionConverter;
		}

		public DefaultEntityRangeBuilder(Schema.Schema schema, EntityExpressionConverter<T> expressionConverter = null) :
			this(schema, schema.GetEntityModel<T>(), expressionConverter)
		{
		}

		public void Limit(System.Linq.Expressions.Expression<Func<T, int>> expression)
			=> Limit(ExpressionConverter.Convert(expression));

		public void Offset(System.Linq.Expressions.Expression<Func<T, int>> expression)
			=> Offset(ExpressionConverter.Convert(expression));
	}
}
