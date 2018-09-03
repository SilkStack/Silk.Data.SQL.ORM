namespace Silk.Data.SQL.ORM.Queries
{
	public interface IValueReader
	{
		object Read();
	}

	public interface IValueReader<T>
	{
		T Read();
	}
}
