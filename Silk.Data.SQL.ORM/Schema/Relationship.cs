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
		public ISchemaField LeftRelationship { get; }
		public ISchemaField RightRelationship { get; }
		public EntityFieldJoin LeftJoin { get; }
		public EntityFieldJoin RightJoin { get; }

		public Relationship(Type left, Type right, string name,
			Table table, ISchemaField leftRelationshipField,
			ISchemaField rightRelationshipField,
			EntityFieldJoin leftJoin, EntityFieldJoin rightJoin)
		{
			LeftType = left;
			RightType = right;
			Name = name;
			JunctionTable = table;
			LeftRelationship = leftRelationshipField;
			RightRelationship = rightRelationshipField;
			LeftJoin = leftJoin;
			RightJoin = rightJoin;
		}
	}

	public class Relationship<TLeft, TRight> : Relationship
		where TLeft : class
		where TRight : class
	{
		public Relationship(string name, Table table,
			ISchemaField leftRelationshipField,
			ISchemaField rightRelationshipField,
			EntityFieldJoin leftJoin, EntityFieldJoin rightJoin)
			: base(typeof(TLeft), typeof(TRight), name, table, leftRelationshipField, rightRelationshipField,
				  leftJoin, rightJoin)
		{
		}
	}
}
