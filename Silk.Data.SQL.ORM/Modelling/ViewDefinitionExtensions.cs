using Silk.Data.Modelling;
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

		public static TableDefinition GetTableDefinition(this ViewDefinition viewDefinition, string tableName)
		{
			return viewDefinition.GetSchemaDefinition()
				?.TableDefinitions.FirstOrDefault(q => q.IsEntityTable);
		}

		public static TableDefinition GetEntityTableDefinition(this ViewDefinition viewDefinition)
		{
			var tableDefinition = viewDefinition.GetTableDefinition(viewDefinition.Name);
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
