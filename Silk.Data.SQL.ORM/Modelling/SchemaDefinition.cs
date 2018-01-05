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

		public SchemaDefinition(ViewDefinition viewDefinition,
			Type entityType, Type projectionType)
		{
			ViewDefinition = viewDefinition;
			EntityType = entityType;
			ProjectionType = projectionType;
		}

		public TableDefinition GetEntityTableDefinition(bool autoCreate = false)
		{
			var entityTable = TableDefinitions.FirstOrDefault(q => q.IsEntityTable);
			if (entityTable == null && autoCreate)
			{
				entityTable = new TableDefinition
				{
					IsEntityTable = true,
					TableName = ViewDefinition.Name,
					EntityType = EntityType
				};
				TableDefinitions.Add(entityTable);
			}
			return entityTable;
		}
	}
}
