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
		public IEntityField LeftRelationship { get; }
		public IEntityField RightRelationship { get; }

		public Relationship(Type left, Type right, string name,
			Table table, IEntityField leftRelationshipField,
			IEntityField rightRelationshipField)
		{
			LeftType = left;
			RightType = right;
			Name = name;
			JunctionTable = table;
			LeftRelationship = leftRelationshipField;
			RightRelationship = rightRelationshipField;
		}
	}

	public class Relationship<TLeft, TRight> : Relationship
		where TLeft : class
		where TRight : class
	{
		public new IEntityFieldOfEntity<TLeft> LeftRelationship { get; }
		public new IEntityFieldOfEntity<TRight> RightRelationship { get; }

		public Relationship(string name, Table table,
			IEntityFieldOfEntity<TLeft> leftRelationshipField,
			IEntityFieldOfEntity<TRight> rightRelationshipField)
			: base(typeof(TLeft), typeof(TRight), name, table, leftRelationshipField, rightRelationshipField)
		{
			LeftRelationship = leftRelationshipField;
			RightRelationship = rightRelationshipField;
		}
	}
}
