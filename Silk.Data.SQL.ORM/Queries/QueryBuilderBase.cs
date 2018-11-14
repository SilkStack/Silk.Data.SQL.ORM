using Silk.Data.Modelling;
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

		protected TableExpression Source { get; }

		public Schema.Schema Schema { get; }
		public EntitySchema<T> EntitySchema { get; }

		public QueryBuilderBase(Schema.Schema schema, ObjectReadWriter entityReadWriter = null)
		{
			Schema = schema;
			EntitySchema = schema.GetEntitySchema<T>();
			if (EntitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			Source = QueryExpression.Table(EntitySchema.EntityTable.TableName);
			_entityReadWriter = entityReadWriter;
		}

		public QueryBuilderBase(EntitySchema<T> schema, ObjectReadWriter entityReadWriter = null)
		{
			EntitySchema = schema;
			Schema = schema.Schema;
			Source = QueryExpression.Table(EntitySchema.EntityTable.TableName);
			_entityReadWriter = entityReadWriter;
		}

		public abstract QueryExpression BuildQuery();
	}

	public abstract class QueryBuilderBase<TLeft, TRight> : IQueryBuilder
		where TLeft : class
		where TRight : class
	{
		private ExpressionConverter<TLeft, TRight> _expressionConverter;
		public ExpressionConverter<TLeft, TRight> ExpressionConverter
		{
			get
			{
				if (_expressionConverter == null)
					_expressionConverter = new ExpressionConverter<TLeft, TRight>(Schema, Relationship.Name);
				return _expressionConverter;
			}
		}

		protected TableExpression Source { get; }

		public Schema.Schema Schema { get; }
		public Relationship<TLeft, TRight> Relationship { get; }
		public EntitySchema<TLeft> LeftSchema { get; }
		public EntitySchema<TRight> RightSchema { get; }

		public QueryBuilderBase(Schema.Schema schema, string relationshipName)
		{
			Schema = schema;
			Relationship = schema.GetRelationship<TLeft, TRight>(relationshipName);
			if (Relationship == null)
				throw new Exception("Relationship isn't configured in schema.");

			Source = QueryExpression.Table(Relationship.JunctionTable.TableName);
			LeftSchema = Schema.GetEntitySchema<TLeft>();
			RightSchema = Schema.GetEntitySchema<TRight>();
		}

		public QueryBuilderBase(Relationship<TLeft, TRight> relationship)
		{
			Relationship = relationship;
			Schema = relationship.Schema;
			Source = QueryExpression.Table(relationship.JunctionTable.TableName);
			LeftSchema = Schema.GetEntitySchema<TLeft>();
			RightSchema = Schema.GetEntitySchema<TRight>();
		}

		public abstract QueryExpression BuildQuery();
	}
}
