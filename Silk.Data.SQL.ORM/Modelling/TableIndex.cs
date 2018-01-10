using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class TableIndex
	{
		public bool UniqueConstraint { get; private set; }
		public IReadOnlyCollection<DataField> Fields { get; private set; }

		public TableIndex(bool uniqueConstraint, IReadOnlyCollection<DataField> fields)
		{
			UniqueConstraint = uniqueConstraint;
			Fields = fields;
		}
	}
}
