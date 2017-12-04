using System;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class DomainDefinition
	{
		public List<Type> EntityTypes { get; } = new List<Type>();
		public List<SchemaDefinition> SchemaDefinitions { get; } = new List<SchemaDefinition>();
		public bool IsReadOnly { get; set; }
	}
}
