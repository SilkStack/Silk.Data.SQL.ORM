using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	public class SchemaIndex : IIndex
	{
		public string IndexName { get; }
		public bool HasUniqueConstraint { get; }
		public IEntityField[] EntityFields { get; }
		public Table Table { get; }
		public string[] ColumnNames { get; }
		public string SourceName => Table.TableName;

		public SchemaIndex(string indexName, bool hasUniqueConstraint, IEntityField[] entityFields,
			Table table)
		{
			IndexName = indexName;
			HasUniqueConstraint = hasUniqueConstraint;
			EntityFields = entityFields;
			Table = table;
			ColumnNames = entityFields.SelectMany(field => field.Columns)
				.Select(column => column.ColumnName).ToArray();
		}
	}
}
