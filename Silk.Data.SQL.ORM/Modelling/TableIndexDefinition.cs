namespace Silk.Data.SQL.ORM.Modelling
{
	public class TableIndexDefinition
	{
		public TableIndexDefinition(params string[] columns)
		{
			Columns = columns;
		}

		public TableIndexDefinition(bool uniqueConstraint, params string[] columns)
		{
			Columns = columns;
			UniqueConstraint = uniqueConstraint;
		}

		public bool UniqueConstraint { get; }
		public string[] Columns { get; }
	}
}
