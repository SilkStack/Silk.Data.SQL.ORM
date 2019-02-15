using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Describes the structure of a table.
	/// </summary>
	public class TableSchema
	{
		public IReadOnlyList<Column> Columns { get; }

		public TableSchema(IEnumerable<Column> columns)
		{
			Columns = columns.ToArray();
		}
	}
}
