using System;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class Relationship
	{
		public Schema Schema { get; internal set; }
		public Type LeftType { get; }
		public Type RightType { get; }
		public string Name { get; }
		public Table JunctionTable { get; }
		public RelationshipField[] RelationshipFields { get; }

		public Relationship(Type left, Type right, string name,
			Table table, Column[] leftColumns, Column[] rightColumns)
		{
			LeftType = left;
			RightType = right;
			Name = name;
			JunctionTable = table;

			RelationshipFields = new[]
			{
				new RelationshipField(leftColumns),
				new RelationshipField(rightColumns)
			};
		}
	}

	public class Relationship<TLeft, TRight> : Relationship
		where TLeft : class
		where TRight : class
	{
		public Relationship(string name, Table table, Column[] leftColumns, Column[] rightColumns)
			: base(typeof(TLeft), typeof(TRight), name, table, leftColumns, rightColumns)
		{
		}
	}
}
