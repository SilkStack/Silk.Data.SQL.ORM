using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Queries.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM
{
	public static class SchemaExtensions
	{
		private static void SanityCheckArgs<T>(EntitySchema<T> schema, T[] entities, bool primaryKeyRequired = true)
		{
			if (entities == null || entities.Length < 1)
				throw new Exception("At least 1 entity must be provided.");

			if (primaryKeyRequired && !schema.EntityFields.Any(q => q.IsPrimaryKey))
				throw new Exception("Entity type must have a primary key to generate update statements.");
		}

		public static QueryExpression CreateDeleteQuery<T>(this EntitySchema<T> schema, params T[] entities)
			where T : class
		{
			SanityCheckArgs(schema, entities);

			return new CompositeQueryExpression(BuildExpressions());

			IEnumerable<QueryExpression> BuildExpressions()
			{
				foreach (var entity in entities)
				{
					var queryBuilder = new DeleteBuilder<T>(schema);
					foreach (var field in schema.EntityFields)
					{
						if (field.IsPrimaryKey)
							queryBuilder.AndWhere(field.GetFieldValuePair(entity), ComparisonOperator.AreEqual);
					}
					yield return queryBuilder.BuildQuery();
				}
			}
		}

		public static QueryExpression CreateDeleteQuery<T>(this Schema.Schema schema, params T[] entities)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			return entitySchema.CreateDeleteQuery(entities);
		}

		public static QueryExpression CreateUpdateQuery<T>(this EntitySchema<T> schema, params T[] entities)
			where T : class
		{
			SanityCheckArgs(schema, entities);

			return new CompositeQueryExpression(BuildExpressions());

			IEnumerable<QueryExpression> BuildExpressions()
			{
				foreach (var entity in entities)
				{
					var queryBuilder = new UpdateBuilder<T>(schema);
					foreach (var field in schema.EntityFields)
					{
						if (field.IsPrimaryKey)
							queryBuilder.AndWhere(field.GetFieldValuePair(entity), ComparisonOperator.AreEqual);
						else
							queryBuilder.Set(field.GetFieldValuePair(entity));
					}
					yield return queryBuilder.BuildQuery();
				}
			}
		}

		public static QueryExpression CreateUpdateQuery<T>(this Schema.Schema schema, params T[] entities)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			return entitySchema.CreateUpdateQuery(entities);
		}
	}
}
