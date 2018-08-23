﻿namespace Silk.Data.SQL.ORM.Schema
{
	public class EntityFieldJoin : ITableJoin
	{
		public string TableName { get; }
		public string TableAlias { get; }
		public string SourceName { get; }
		public string[] LeftColumns { get; }
		public string[] RightColumns { get; }
		public EntityField EntityField { get; }
		public EntityFieldJoin[] DependencyJoins { get; }

		public EntityFieldJoin(string tableName, string tableAlias,
			string sourceName, string[] leftColumns, string[] rightColumns,
			EntityField entityField, EntityFieldJoin[] dependencyJoins = null)
		{
			TableName = tableName;
			TableAlias = tableAlias;
			SourceName = sourceName;
			LeftColumns = leftColumns;
			RightColumns = rightColumns;
			EntityField = entityField;
			DependencyJoins = dependencyJoins ?? new EntityFieldJoin[0];
		}
	}
}
