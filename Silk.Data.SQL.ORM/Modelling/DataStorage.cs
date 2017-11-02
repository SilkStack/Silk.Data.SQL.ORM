namespace Silk.Data.SQL.ORM.Modelling
{
	public class DataStorage
	{
		public string ColumnName { get; }
		public SqlDataType DataType { get; }
		public Table Table { get; }
		public bool IsAutoIncrement { get; }
		public bool IsPrimaryKey { get; }
		public bool IsAutoGenerate { get; }
		public bool IsNullable { get; }

		public DataStorage(string columnName, SqlDataType dataType,
			Table table, bool isPrimaryKey, bool isAutoIncrement,
			bool isAutoGenerate, bool isNullable)
		{
			ColumnName = columnName;
			DataType = dataType;
			Table = table;
			IsPrimaryKey = isPrimaryKey;
			IsAutoIncrement = isAutoIncrement;
			IsAutoGenerate = isAutoGenerate;
			IsNullable = isNullable;
		}
	}
}
