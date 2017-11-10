using Silk.Data.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling
{
	public static class ViewDefinitionExtensions
	{
		public static SchemaDefinition GetSchemaDefinition(this ViewDefinition viewDefinition, bool createIfNotExists = false)
		{
			var domain = viewDefinition.UserData.OfType<DomainDefinition>().First();
			var ret = domain
				.SchemaDefinitions.FirstOrDefault(q => q.ViewDefinition == viewDefinition);
			if (ret == null && createIfNotExists)
			{
				ret = new SchemaDefinition(viewDefinition);
				domain.SchemaDefinitions.Add(ret);
			}
			return ret;
		}

		public static SchemaDefinition GetSchemaDefinitionFor(this ViewDefinition viewDefinition, Type entityType)
		{
			var domain = viewDefinition.UserData.OfType<DomainDefinition>().First();
			return domain
				.SchemaDefinitions.FirstOrDefault(q => q.EntityType == entityType);
		}

		public static IEnumerable<TableDefinition> GetTableDefinitions(this ViewDefinition viewDefinition)
		{
			return viewDefinition.GetSchemaDefinition()
				?.TableDefinitions;
		}

		public static TableDefinition GetTableDefinition(this ViewDefinition viewDefinition, string tableName, bool isEntityTable = false)
		{
			return viewDefinition.GetSchemaDefinition()
				?.TableDefinitions.FirstOrDefault(q => q.IsEntityTable == isEntityTable && q.TableName == tableName);
		}

		public static void AddTableDefinition(this ViewDefinition viewDefinition, TableDefinition tableDefinition)
		{
			viewDefinition.GetSchemaDefinition(true).TableDefinitions.Add(tableDefinition);
		}

		public static TableDefinition GetEntityTableDefinition(this ViewDefinition viewDefinition)
		{
			var tableDefinition = viewDefinition.GetTableDefinition(viewDefinition.Name, true);
			if (tableDefinition == null)
			{
				tableDefinition = new TableDefinition
				{
					TableName = viewDefinition.Name,
					IsEntityTable = true
				};
				viewDefinition.GetSchemaDefinition(true).TableDefinitions.Add(tableDefinition);
			}
			return tableDefinition;
		}
	}
}
