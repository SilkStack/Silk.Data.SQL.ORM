namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// A column definition on a table.
	/// </summary>
	public class Column
	{
		public string Name { get; }
		public SqlDataType DataType { get; }
		public bool IsNullable { get; }

		public Column(string name, SqlDataType dataType, bool isNullable)
		{
			Name = name;
			DataType = dataType;
			IsNullable = isNullable;
		}
	}
}
