using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class CreateEntityTableBuilder<T> : IQueryBuilder
		where T : class
	{
		public Schema.Schema Schema { get; }
		public EntitySchema<T> EntitySchema { get; }

		public CreateEntityTableBuilder(Schema.Schema schema)
		{
			Schema = schema;
			EntitySchema = schema.GetEntitySchema<T>();
			if (EntitySchema == null)
				throw new Exception("Entity isn't configured in schema.");
		}

		public CreateEntityTableBuilder(EntitySchema<T> schema)
		{
			Schema = schema.Schema;
			EntitySchema = schema;
		}

		public QueryExpression BuildQuery()
		{
			return new CompositeQueryExpression(
				new QueryExpression[] { GetCreateTableExpressions() }
					.Concat(GetCreateIndexExpressions())
				);
		}

		private IEnumerable<QueryExpression> GetCreateIndexExpressions()
		{
			foreach (var index in EntitySchema.Indexes)
			{
				yield return QueryExpression.CreateIndex(
					EntitySchema.EntityTable.TableName,
					index.HasUniqueConstraint,
					index.ColumnNames
					);
			}
		}

		private CreateTableExpression GetCreateTableExpressions()
		{
			return QueryExpression.CreateTable(
				EntitySchema.EntityTable.TableName,
				EntitySchema.SchemaFields.Select(q =>
					Schema.GetFieldOperations(q).Expressions.DefineColumn()
					).ToArray()
				);
		}
	}
}
