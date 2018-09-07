using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;

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

	public abstract class QueryBuilderBase<TLeft, TRight> : IQueryBuilder
		where TLeft : class
		where TRight : class
	{
		protected TableExpression Source { get; }

		public Schema.Schema Schema { get; }
		public Relationship<TLeft, TRight> Relationship { get; }

		public QueryBuilderBase(Schema.Schema schema, string relationshipName)
		{
			Schema = schema;
			Relationship = schema.GetRelationship<TLeft, TRight>(relationshipName);
			if (Relationship == null)
				throw new Exception("Relationship isn't configured in schema.");

			Source = QueryExpression.Table(Relationship.JunctionTable.TableName);
		}

		public QueryBuilderBase(Relationship<TLeft, TRight> relationship)
		{
			Relationship = relationship;
			Schema = relationship.Schema;
			Source = QueryExpression.Table(relationship.JunctionTable.TableName);
		}

		public abstract QueryExpression BuildQuery();
	}
}
