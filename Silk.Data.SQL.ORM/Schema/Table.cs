using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	public class Table
	{
		public string TableName { get; }

		public Column[] Columns { get; }

		public Table(string name, IEnumerable<Column> columns)
		{
			TableName = name;
			Columns = columns.ToArray();
		}
	}
}
