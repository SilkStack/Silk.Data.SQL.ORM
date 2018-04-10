namespace Silk.Data.SQL.ORM.Schema
{
	public class Index
	{
		public string Name { get; }
		public IndexOption Option { get; }

		public Index(string name, IndexOption option = IndexOption.None)
		{
			Name = name;
			Option = option;
		}
	}

	public enum IndexOption
	{
		None,
		Unique,
		FullText
	}
}
