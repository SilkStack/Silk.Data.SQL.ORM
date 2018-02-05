using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.NewModelling;
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
			builder.AddEntityType<TSource>();
			addBuildersFunc?.Invoke(builder);
			var dataDomain = builder.Build();
			return dataDomain.GetEntityModel<TSource>();
		}

		public static EntitySchema<TSource> CreateDomainAndSchema<TSource>(Action<DataDomainBuilder> addBuildersFunc = null)
			where TSource : new()
		{
			var builder = new DataDomainBuilder();
			builder.AddEntityType<TSource>();
			addBuildersFunc?.Invoke(builder);
			var dataDomain = builder.Build();
			return dataDomain.GetEntitySchema<TSource>();
		}
	}
}
