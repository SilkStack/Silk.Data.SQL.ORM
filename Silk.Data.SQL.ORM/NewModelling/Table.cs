using System;

namespace Silk.Data.SQL.ORM.NewModelling
{
	/// <summary>
	/// A table in a data domain.
	/// </summary>
	public class Table
	{
		public string TableName { get; }
		public bool IsEntityTable { get; }
		public Type EntityType { get; }
		public IDataField[] Fields { get; }

		public Table(string tableName, bool isEntityTable, IDataField[] dataFields, Type entityType)
		{
			if (isEntityTable && entityType == null)
				throw new ArgumentNullException(nameof(entityType), "Entity tables must specify an entity type.");

			TableName = tableName;
			IsEntityTable = isEntityTable;
			EntityType = entityType;
			Fields = dataFields;
		}
	}
}
