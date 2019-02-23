using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Schema;
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
		}

		private class WithGuidPK
		{
			public Guid Id { get; private set; }
		}
	}
}
