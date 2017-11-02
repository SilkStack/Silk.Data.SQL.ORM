using Silk.Data.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class SchemaDefinition
	{
		public ViewDefinition ViewDefinition { get; }
		public Type EntityType { get; }
		public Type ProjectionType { get; }
		public List<TableDefinition> TableDefinitions { get; } = new List<TableDefinition>();

		public SchemaDefinition(ViewDefinition viewDefinition)
		{
			ViewDefinition = viewDefinition;
			EntityType = viewDefinition.UserData.OfType<Type>().First();
			ProjectionType = viewDefinition.UserData.OfType<Type>().Skip(1).FirstOrDefault();
		}
	}
}
