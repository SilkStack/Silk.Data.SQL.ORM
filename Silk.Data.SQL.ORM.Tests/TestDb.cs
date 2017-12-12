using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.SQLite3;
using System;

namespace Silk.Data.SQL.ORM.Tests
{
	public static class TestDb
	{
		public static SQLite3DataProvider Provider { get; } =
			new SQLite3DataProvider(":memory:");

		public static EntityModel<TSource> CreateDomainAndModel<TSource>(Action<DataDomainBuilder> addBuildersFunc = null)
			where TSource : new()
		{
			var builder = new DataDomainBuilder();
			builder.AddDataEntity<TSource>();
			addBuildersFunc?.Invoke(builder);
			var dataDomain = builder.Build();
			return dataDomain.GetEntityModel<TSource>();
		}

		public static EntityModel<TSource,TView> CreateDomainAndModel<TSource,TView>(Action<DataDomainBuilder> addBuildersFunc = null)
			where TSource : new()
			where TView : new()
		{
			var builder = new DataDomainBuilder();
			builder.AddDataEntity<TSource, TView>();
			addBuildersFunc?.Invoke(builder);
			var dataDomain = builder.Build();
			return dataDomain.GetEntityModel<TSource>() as EntityModel<TSource, TView>;
		}
	}
}
