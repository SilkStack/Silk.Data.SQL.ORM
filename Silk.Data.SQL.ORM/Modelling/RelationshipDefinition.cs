using System;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class RelationshipDefinition
	{
		public Type EntityType { get; set; }
		public Type ProjectionType { get; set; }
		public string RelationshipField { get; set; }
		public RelationshipType RelationshipType { get; set; }
	}
}
