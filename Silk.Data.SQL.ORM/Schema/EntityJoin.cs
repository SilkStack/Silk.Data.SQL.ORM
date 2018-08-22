using System;

namespace Silk.Data.SQL.ORM.Schema
{
	public class EntityJoin : ITableJoin
	{
		public string TableName { get; }
		public string TableAlias { get; }
		public string SourceName { get; }
		public string[] LeftColumns { get; }
		public string[] RightColumns { get; }
		public Type EntityType { get; }

		public EntityJoin(string tableName, string tableAlias,
			string sourceName, string[] leftColumns, string[] rightColumns,
			Type entityType)
		{
			TableName = tableName;
			TableAlias = tableAlias;
			SourceName = sourceName;
			LeftColumns = leftColumns;
			RightColumns = rightColumns;
			EntityType = entityType;
		}
	}
}
