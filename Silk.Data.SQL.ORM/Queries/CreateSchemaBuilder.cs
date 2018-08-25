using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silk.Data.SQL.ORM.Queries
{
	public class CreateSchemaBuilder
	{
		protected Dictionary<string, EntityField[]> Fields { get; }
			= new Dictionary<string, EntityField[]>();
		protected Dictionary<string, IIndex[]> Indexes { get; }
			= new Dictionary<string, IIndex[]>();

		public QueryExpression BuildQuery()
		{
			return new CompositeQueryExpression(
				GetCreateTableExpressions()
					.Concat(GetCreateIndexExpressions())
				);
		}

		private IEnumerable<QueryExpression> GetCreateIndexExpressions()
		{
			foreach (var kvp in Indexes)
			{
				foreach (var index in kvp.Value)
				{
					//  todo: support the custom index name specified
					yield return QueryExpression.CreateIndex(
						index.SourceName,
						index.HasUniqueConstraint,
						index.ColumnNames
						);
				}
			}
		}

		private IEnumerable<QueryExpression> GetCreateTableExpressions()
		{
			foreach (var kvp in Fields)
			{
				yield return QueryExpression.CreateTable(
					kvp.Key,
					kvp.Value.SelectMany(field =>
						field.Columns.Select(column => QueryExpression.DefineColumn(
							column.ColumnName, column.DataType, column.IsNullable,
							field.PrimaryKeyGenerator == PrimaryKeyGenerator.ServerGenerated,
							field.IsPrimaryKey
							))
					).ToArray());
			}
		}
	}

	public class EntityCreateSchemaBuilder<T> : CreateSchemaBuilder
		where T : class
	{
		public Schema.Schema Schema { get; }
		public EntitySchema<T> EntitySchema { get; }

		public EntityCreateSchemaBuilder(Schema.Schema schema)
		{
			Schema = schema;
			EntitySchema = schema.GetEntitySchema<T>();
			if (EntitySchema == null)
				throw new Exception("Entity isn't configured in schema.");
			AddEntityTable();
		}

		private void AddEntityTable()
		{
			Fields.Add(EntitySchema.EntityTable.TableName,
				EntitySchema.EntityFields);
			Indexes.Add(EntitySchema.EntityTable.TableName,
				EntitySchema.Indexes);
		}
	}
}
