namespace Silk.Data.SQL.ORM.Modelling
{
	public class DataStorage
	{
		public string ColumnName { get; }
		public SqlDataType DataType { get; }

		public DataStorage(string columnName, SqlDataType dataType)
		{
			ColumnName = columnName;
			DataType = dataType;
		}
	}
}
