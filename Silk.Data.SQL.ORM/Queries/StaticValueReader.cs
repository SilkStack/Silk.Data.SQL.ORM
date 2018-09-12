namespace Silk.Data.SQL.ORM.Queries
{
	public class StaticValueReader<T> : IValueReader<T>
	{
		private readonly T _value;

		public StaticValueReader(T value)
		{
			_value = value;
		}

		public T Read() => _value;
	}
}
