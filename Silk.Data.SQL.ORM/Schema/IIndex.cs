namespace Silk.Data.SQL.ORM.Schema
{
	public interface IIndex
	{
		string IndexName { get; }
		bool HasUniqueConstraint { get; }
		string[] ColumnNames { get; }
		string SourceName { get; }
	}
}
