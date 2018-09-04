using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Silk.Data.SQL.ORM.Queries
{
	public abstract class QueryBuilderBase<T> : IQueryBuilder
		where T : class
	{
		private ExpressionConverter<T> _expressionConverter;
		public ExpressionConverter<T> ExpressionConverter
		{
			get
			{
				if (_expressionConverter == null)
					_expressionConverter = new ExpressionConverter<T>(Schema);
				return _expressionConverter;
			}
		}

		protected TableExpression Source { get; }

		public Schema.Schema Schema { get; }
		public EntitySchema<T> EntitySchema { get; }

		public QueryBuilderBase(Schema.Schema schema)
		{
			Schema = schema;
			EntitySchema = schema.GetEntitySchema<T>();
			if (EntitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			Source = QueryExpression.Table(EntitySchema.EntityTable.TableName);
		}

		public QueryBuilderBase(EntitySchema<T> schema)
		{
			EntitySchema = schema;
			Schema = schema.Schema;
			Source = QueryExpression.Table(EntitySchema.EntityTable.TableName);
		}

		public abstract QueryExpression BuildQuery();
	}
}
