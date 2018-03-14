namespace Silk.Data.SQL.ORM.Schema
{
	public class Column
	{
		public string ColumnName { get; }
		public SqlDataType SqlDataType { get; }

		public Column(string columnName, SqlDataType sqlDataType)
		{
			ColumnName = columnName;
			SqlDataType = sqlDataType;
		}
	}
}