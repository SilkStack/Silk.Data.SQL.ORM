using System;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class RelationshipDefinition
	{
		public Type EntityType { get; set; }
		public Type ProjectionType { get; set; }
		public string RelationshipField { get; set; }
		public RelationshipType RelationshipType { get; set; }
		public List<TableReferenceDefinition> TableReferences { get; private set; }

		public TableReferenceDefinition CreateTableReference()
		{
			if (TableReferences == null)
				TableReferences = new List<TableReferenceDefinition>();
			var newReference = new TableReferenceDefinition();
			TableReferences.Add(newReference);
			return newReference;
		}
	}
}
