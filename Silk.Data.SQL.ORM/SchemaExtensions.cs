using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Queries.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silk.Data.SQL.ORM
{
	public static class SchemaExtensions
	{
		public static QueryExpression CreateUpdateQuery<T>(this EntitySchema<T> schema, params T[] entities)
			where T : class
		{
			if (entities == null || entities.Length < 1)
				throw new Exception("At least 1 entity must be provided.");

			if (!schema.EntityFields.Any(q => q.IsPrimaryKey))
				throw new Exception("Entity type must have a primary key to generate update statements.");

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
