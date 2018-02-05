using System;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Modelling
{
	/// <summary>
	/// Describes a table.
	/// </summary>
	public class TableDefinition
	{
		public string TableName { get; set; }
		public bool IsEntityTable { get; set; }
		public Type EntityType { get; set; }
		public bool IsJoinTable { get; set; }
		public List<FieldDefinition> Fields { get; } = new List<FieldDefinition>();

		public List<Type> JoinEntityTypes { get; } = new List<Type>();
		public List<TableIndexDefinition> Indexes { get; } = new List<TableIndexDefinition>();
	}
}
