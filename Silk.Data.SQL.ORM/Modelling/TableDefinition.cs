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
		public bool IsJoinTable { get; set; }
		public List<Type> JoinEntityTypes { get; } = new List<Type>();
		public List<ViewFieldDefinition> Fields { get; } = new List<ViewFieldDefinition>();
		public List<TableIndexDefinition> Indexes { get; } = new List<TableIndexDefinition>();
	}
}
