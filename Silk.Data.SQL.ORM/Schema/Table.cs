namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Database table.
	/// </summary>
	public class Table
	{
		/// <summary>
		/// Gets the table's name.
		/// </summary>
		public string TableName { get; }

		/// <summary>
		/// Gets an array of columns in the table.
		/// </summary>
		public Column[] Columns { get; }

		public Table(string tableName, Column[] columns)
		{
			TableName = tableName;
			Columns = columns;
		}
	}
}
