namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Database column.
	/// </summary>
	public class Column
	{
		public string ColumnName { get; }
		public SqlDataType DataType { get; }
		public bool IsNullable { get; }
		public string SourceName { get; }

		public Column(string columnName, SqlDataType dataType, bool isNullable, string sourceName)
		{
			ColumnName = columnName;
			DataType = dataType;
			IsNullable = isNullable;
			SourceName = sourceName;
		}
	}
}
