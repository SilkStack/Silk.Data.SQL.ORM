namespace Silk.Data.SQL.ORM.Modelling
{
	public class DataStorage
	{
		public string ColumnName { get; }
		public SqlDataType DataType { get; }
		public TableSchema Table { get; }
		public bool IsAutoIncrement { get; }
		public bool IsPrimaryKey { get; }

		public DataStorage(string columnName, SqlDataType dataType,
			TableSchema table)
		{
			ColumnName = columnName;
			DataType = dataType;
			Table = table;
		}
	}
}
