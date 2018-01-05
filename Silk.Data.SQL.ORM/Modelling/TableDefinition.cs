using Silk.Data.Modelling;
using System;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class TableDefinition
	{
		public string TableName { get; set; }
		public bool IsEntityTable { get; set; }
		public Type EntityType { get; set; }
		public List<ViewFieldDefinition> Fields { get; } = new List<ViewFieldDefinition>();
	}
}
