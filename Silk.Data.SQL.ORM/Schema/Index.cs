using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Schema
{
	public class Index
	{
		public string IndexName { get; }
		public bool HasUniqueConstraint { get; }
		public IReadOnlyList<EntityField> Fields { get; }

		public Index(string indexName, bool hasUniqueConstraint, IReadOnlyList<EntityField> fields)
		{
			IndexName = indexName;
			HasUniqueConstraint = hasUniqueConstraint;
			Fields = fields;
		}
	}
}
