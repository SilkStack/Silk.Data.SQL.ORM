using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public abstract class QueryBuilderBase<T> : IQueryBuilder
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

		protected ITableJoin[] ConcatUniqueJoins(params EntityFieldJoin[][] joins)
		{
			IEnumerable<ITableJoin> result = new ITableJoin[0];
			foreach (var joinArray in joins)
			{
				if (joinArray == null || joinArray.Length < 1)
					continue;
				result = result.Concat(joinArray);
			}
			return result
				.GroupBy(join => join)
				.Select(joinGroup => joinGroup.First())
				.ToArray();
		}

		public abstract QueryExpression BuildQuery();
	}
}
