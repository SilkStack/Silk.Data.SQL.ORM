using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class RelationshipBuilder
	{
		public Type Left { get; }
		public Type Right { get; }
		public string Name { get; set; }

		public RelationshipBuilder(Type left, Type right)
		{
			Left = left;
			Right = right;
		}

		public abstract Relationship Build(EntitySchema[] entitySchemas);
	}

	public class RelationshipBuilder<TLeft, TRight> : RelationshipBuilder
		where TLeft : class
		where TRight : class
	{
		public RelationshipBuilder() : base(typeof(TLeft), typeof(TRight)) { }

		public override Relationship Build(EntitySchema[] entitySchemas)
		{
			var leftEntitySchema = entitySchemas.OfType<EntitySchema<TLeft>>().First();
			var rightEntitySchema = entitySchemas.OfType<EntitySchema<TRight>>().First();

			var leftPrimaryKeys = leftEntitySchema.EntityFields.Where(q => q.IsPrimaryKey).ToArray();
			var rightPrimaryKeys = rightEntitySchema.EntityFields.Where(q => q.IsPrimaryKey).ToArray();

			if (leftPrimaryKeys.Length == 0 || rightPrimaryKeys.Length == 0)
				throw new Exception("Related entities must have primary keys.");

			var leftColumns = leftPrimaryKeys
				.SelectMany(q => q.Columns)
				.Select(q => new Column($"{leftEntitySchema.EntityTable.TableName}_{q.ColumnName}", q.DataType, false))
				.ToArray();
			var rightColumns = rightPrimaryKeys
				.SelectMany(q => q.Columns)
				.Select(q => new Column($"{rightEntitySchema.EntityTable.TableName}_{q.ColumnName}", q.DataType, false))
				.ToArray();

			var columns = leftColumns.Concat(rightColumns).ToArray();

			var table = new Table(Name, columns);

			return new Relationship<TLeft, TRight>(Name, table, leftColumns, rightColumns);
		}
	}
}
