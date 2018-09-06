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

		private Dictionary<string, ITableField[]> _fields
			= new Dictionary<string, ITableField[]>();
		private Dictionary<string, IIndex[]> _indexes
			= new Dictionary<string, IIndex[]>();

		public CreateTableBuilder(Schema.Schema schema)
		{
			Schema = schema;
			EntitySchema = schema.GetEntitySchema<T>();
			if (EntitySchema == null)
				throw new Exception("Entity isn't configured in schema.");
			AddEntityTable();
		}

		public CreateTableBuilder(EntitySchema<T> schema)
		{
			Schema = schema.Schema;
			EntitySchema = schema;
			AddEntityTable();
		}

		private void AddEntityTable()
		{
			_fields.Add(EntitySchema.EntityTable.TableName,
				EntitySchema.EntityFields);
			_indexes.Add(EntitySchema.EntityTable.TableName,
				EntitySchema.Indexes);
		}

		public QueryExpression BuildQuery()
		{
			return new CompositeQueryExpression(
				GetCreateTableExpressions()
					.Concat(GetCreateIndexExpressions())
				);
		}

		private IEnumerable<QueryExpression> GetCreateIndexExpressions()
		{
			foreach (var kvp in _indexes)
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
			_fields.Add(Relationship.JunctionTable.TableName,
				Relationship.RelationshipFields);
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
