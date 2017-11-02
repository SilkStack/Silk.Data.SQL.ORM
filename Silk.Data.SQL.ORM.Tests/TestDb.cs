using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.SQLite3;

namespace Silk.Data.SQL.ORM.Tests
{
	public static class TestDb
	{
		public static SQLite3DataProvider Provider { get; } =
			new SQLite3DataProvider(":memory:");

		public static EntityModel<TSource> CreateDomainAndModel<TSource>()
			where TSource : new()
		{
			var builder = new DataDomainBuilder();
			builder.AddDataEntity<TSource>();
			var dataDomain = builder.Build();
			return dataDomain.GetEntityModel<TSource>();
		}

		public static EntityModel<TSource,TView> CreateDomainAndModel<TSource,TView>()
			where TSource : new()
			where TView : new()
		{
			var builder = new DataDomainBuilder();
			builder.AddDataEntity<TSource, TView>();
			var dataDomain = builder.Build();
			return dataDomain.GetEntityModel<TSource>() as EntityModel<TSource, TView>;
		}
	}
}
