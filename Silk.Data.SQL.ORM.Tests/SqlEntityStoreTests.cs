using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.SQLite3;
using System;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class SqlEntityStoreTests
	{
		[TestMethod]
		public void Insert_Populates_Guid_Primary_Key()
		{
			var schema = new SchemaBuilder()
				.Define<WithGuidPK>()
				.Build();
			var sqlEntityStore = new SqlEntityStore<WithGuidPK>(schema, null);
			var entity = new WithGuidPK();
			sqlEntityStore.Insert(entity);

			Assert.AreNotEqual(Guid.Empty, entity.Id);
		}

		[TestMethod]
		public void Insert_Maps_Generated_Primary_Key()
		{
			var entity = new WithIntPK();

			using (var dataProvider = new SQLite3DataProvider("Data Source=:memory:;Mode=Memory"))
			{
				var schema = new SchemaBuilder()
					.Define<WithIntPK>()
					.Build();

				var table = new EntityTable<WithIntPK>(schema, dataProvider);
				var sqlEntityStore = new SqlEntityStore<WithIntPK>(schema, dataProvider);

				table.CreateTable().Execute();
				sqlEntityStore.Insert(entity).Execute();
			}

			Assert.AreNotEqual(0, entity.Id);
		}

		private class WithGuidPK
		{
			public Guid Id { get; private set; }
		}

		private class WithIntPK
		{
			public int Id { get; private set; }
			public int SomeData { get; set; }
		}
	}
}
