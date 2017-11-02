using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class DomainDefinition
	{
		public List<SchemaDefinition> SchemaDefinitions { get; } = new List<SchemaDefinition>();
	}
}
