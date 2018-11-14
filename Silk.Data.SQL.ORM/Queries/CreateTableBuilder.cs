using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class CreateTableBuilder<T> : IQueryBuilder
		where T : class
	{
		public Schema.Schema Schema { get; }
		public EntitySchema<T> EntitySchema { get; }

		public CreateTableBuilder(Schema.Schema schema)
		{
			Schema = schema;
			EntitySchema = schema.GetEntitySchema<T>();
			if (EntitySchema == null)
				throw new Exception("Entity isn't configured in schema.");
		}

		public CreateTableBuilder(EntitySchema<T> schema)
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
					q.ExpressionFactory.DefineColumn()
					).ToArray()
				);
		}
	}

	public class CreateTableBuilder<TLeft, TRight> : IQueryBuilder
		where TLeft : class
		where TRight : class
	{
		public Schema.Schema Schema { get; }
		public Relationship<TLeft, TRight> Relationship { get; }

		private Dictionary<string, ITableField[]> _fields
			= new Dictionary<string, ITableField[]>();

		public CreateTableBuilder(Schema.Schema schema, string name)
		{
			Schema = schema;
			Relationship = schema.GetRelationship<TLeft, TRight>(name);
			if (Relationship == null)
				throw new Exception("Relationship isn't configured in schema.");
			AddEntityTable();
		}

		public CreateTableBuilder(Relationship<TLeft, TRight> relationship)
		{
			Schema = relationship.Schema;
			Relationship = relationship;
			AddEntityTable();
		}

		private void AddEntityTable()
		{
			throw new NotImplementedException();
			//_fields.Add(Relationship.JunctionTable.TableName, new ITableField[] {
			//	Relationship.LeftRelationship,
			//	Relationship.RightRelationship
			//});
		}

		public QueryExpression BuildQuery()
		{
			return new CompositeQueryExpression(
				GetCreateTableExpressions()
				);
		}

		private IEnumerable<QueryExpression> GetCreateTableExpressions()
		{
			foreach (var kvp in _fields)
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
}
