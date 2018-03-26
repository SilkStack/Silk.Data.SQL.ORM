namespace Silk.Data.SQL.ORM.Schema
{
	public class Column
	{
		public string ColumnName { get; }
		public SqlDataType SqlDataType { get; }
		public bool IsPrimaryKey { get; }
		public bool IsServerGenerated { get; }
		public bool IsClientGenerated { get; }
		public bool IsNullable { get; }

		public Column(string columnName, SqlDataType sqlDataType,
			bool isPrimaryKey = false, bool isServerGenerated = false, bool isClientGenerated = false,
			bool isNullable = false)
		{
			ColumnName = columnName;
			SqlDataType = sqlDataType;
			IsPrimaryKey = isPrimaryKey;
			IsClientGenerated = isClientGenerated;
			IsServerGenerated = isServerGenerated;
			IsNullable = isNullable;
		}
	}
}