using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.NewModelling;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.SQLite3;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests
{
	public static class TestDb
	{
		public static SQLite3DataProvider Provider { get; } =
			new SQLite3DataProvider(":memory:");

		[Obsolete("Move tests away from old APIs")]
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

		public static void ExecuteSql(ORMQuery query)
		{
			new QueryCollection(null).NonResultQuery(query).Execute(Provider);
		}

		public static T ExecuteAndRead<T>(ORMQuery query)
		{
			return new QueryCollection<T>(null, new List<ORMQuery> { query }, new BasicQueryExecutor()).Execute(Provider).FirstOrDefault();
		}
	}
}
