using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Describes the structure of a table.
	/// </summary>
	public class Table
	{
		public string TableName { get; }

		public IReadOnlyList<Column> Columns { get; }

		public Table(string tableName, IEnumerable<Column> columns)
		{
			TableName = tableName;
			Columns = columns.ToArray();
		}
	}
}
