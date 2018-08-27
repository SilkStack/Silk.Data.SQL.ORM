using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class InsertBuilder : IQueryBuilder
	{
		protected string Into { get; set; }
		protected string[] Columns { get; }
		protected List<IValueReader[]> RowValueReaders { get; }
			= new List<IValueReader[]>();

		public InsertBuilder(string[] columns)
		{
			Columns = columns;
		}

		public QueryExpression BuildQuery()
		{
			return QueryExpression.Insert(
				Into,
				Columns,
				RowValueReaders.SelectMany(
					row => row.Select(reader => QueryExpression.Value(reader.Read()) as QueryExpression)
					).ToArray()
				);
		}
	}

	public class InsertBuilder<T> : InsertBuilder
	{
		public InsertBuilder(string[] columns) : base(columns)
		{
		}
	}

	public class EntityInsertBuilder<T> : InsertBuilder<T>
		where T : class
	{
		public Schema.Schema Schema { get; }
		public EntitySchema<T> EntitySchema { get; }

		public EntityInsertBuilder(Schema.Schema schema)
			: base(
				  schema.GetEntitySchema<T>().EntityFields
					.SelectMany(q => q.Columns).Select(q => q.ColumnName).ToArray()
				  )
		{
			Schema = schema;
			EntitySchema = schema.GetEntitySchema<T>();
			if (EntitySchema == null)
				throw new Exception("Entity isn't configured in schema.");
			Into = EntitySchema.EntityTable.TableName;
		}

		private void Add(T instance)
		{
			var row = new List<IValueReader>();
			foreach (var field in EntitySchema.EntityFields)
			{
				foreach (var column in field.Columns)
				{
					row.Add(field.GetValueReader(instance, column));
				}
			}
			RowValueReaders.Add(row.ToArray());
		}

		public void Add(IEnumerable<T> instances)
		{
			foreach (var instance in instances)
			{
				Add(instance);
			}
		}

		public void Add(params T[] instances) => Add((IEnumerable<T>)instances);
	}
}
