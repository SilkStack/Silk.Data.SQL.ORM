namespace Silk.Data.SQL.ORM.Schema
{
	public interface ITableJoin
	{
		string TableName { get; }
		string TableAlias { get; }
		string SourceName { get; }
		string[] LeftColumns { get; }
		string[] RightColumns { get; }
	}
}
