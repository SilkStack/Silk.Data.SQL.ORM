using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class TableSQLTests
	{
		private NewModelling.EntitySchema<BasicSqlTypesModel>
			_simpleEntitySchema = TestDb.CreateDomainAndSchema<BasicSqlTypesModel>();

		[TestMethod]
		public void CreateSimpleTable()
		{
			var table = _simpleEntitySchema.EntityTable;

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			Assert.AreEqual(1, TestDb.ExecuteAndRead<int>(table.TableExists()));
		}

		[TestMethod]
		public void DropSimpleTable()
		{
			var table = _simpleEntitySchema.EntityTable;

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 0)
				TestDb.ExecuteSql(table.CreateTable());
			TestDb.ExecuteSql(table.DropTable());

			Assert.AreEqual(0, TestDb.ExecuteAndRead<int>(table.TableExists()));
		}

		private class BasicSqlTypesModel
		{
			public bool Bit { get; set; }
			public byte TinyInt { get; set; }
			public short SmallInt { get; set; }
			public int Int { get; set; }
			public long BigInt { get; set; }
			public float Float { get; set; }
			public double Double { get; set; }
			public decimal Decimal { get; set; }
			public string Text { get; set; }
			public Guid Guid { get; set; }
			public DateTime DateTime { get; set; }
		}
	}
}
