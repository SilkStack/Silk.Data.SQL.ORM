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

		public Column(string columnName, SqlDataType dataType, bool isNullable)
		{
			ColumnName = columnName;
			DataType = dataType;
			IsNullable = isNullable;
		}
	}
}
