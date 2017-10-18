using Silk.Data.Modelling;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling
{
	public static class ViewDefinitionExtensions
	{
		public static TableDefinition GetTableDefinition(this ViewDefinition viewDefinition, string tableName)
		{
			return viewDefinition.UserData.OfType<TableDefinition>()
				.FirstOrDefault(q => q.TableName == tableName);
		}

		public static TableDefinition GetDefaultTableDefinition(this ViewDefinition viewDefinition)
		{
			var tableDefinition = viewDefinition.GetTableDefinition(viewDefinition.Name);
			if (tableDefinition == null)
			{
				tableDefinition = new TableDefinition
				{
					TableName = viewDefinition.Name
				};
				viewDefinition.UserData.Add(tableDefinition);
			}
			return tableDefinition;
		}
	}
}
