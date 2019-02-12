using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	public class SchemaIndex : IIndex
	{
		public string IndexName { get; }
		public bool HasUniqueConstraint { get; }
		public SchemaField[] SchemaFields { get; }
		public Table Table { get; }
		public string[] ColumnNames { get; }
		public string SourceName => Table.TableName;

		public SchemaIndex(string indexName, bool hasUniqueConstraint, SchemaField[] schemaFields,
			Table table)
		{
			IndexName = indexName;
			HasUniqueConstraint = hasUniqueConstraint;
			SchemaFields = schemaFields;
			Table = table;
			ColumnNames = schemaFields.Select(field => field.Column)
				.Select(column => column.ColumnName).ToArray();
		}
	}
}
