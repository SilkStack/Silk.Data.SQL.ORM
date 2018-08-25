namespace Silk.Data.SQL.ORM.Schema
{
	public interface ITableField
	{
		Column[] Columns { get; }
		PrimaryKeyGenerator PrimaryKeyGenerator { get; }
		bool IsPrimaryKey { get; }
	}
}
