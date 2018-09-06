namespace Silk.Data.SQL.ORM.Schema
{
	public class RelationshipField : ITableField
	{
		public Column[] Columns { get; }
		public PrimaryKeyGenerator PrimaryKeyGenerator => PrimaryKeyGenerator.NotPrimaryKey;
		public bool IsPrimaryKey => false;

		public RelationshipField(Column[] columns)
		{
			Columns = columns;
		}
	}
}
