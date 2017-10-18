using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class TableSQLTests
	{
		private DataModel<BasicSqlTypesModel, BasicSqlTypesModel>
			_simpleDataModel = TypeModeller.GetModelOf<BasicSqlTypesModel>()
				.GetModeller<BasicSqlTypesModel>()
				.CreateDataModel();


		[TestMethod]
		public void CreateSimpleTable()
		{
			var table = _simpleDataModel.Fields.First().Storage.Table;

			if (table.Exists(TestDb.Provider))
				table.Drop(TestDb.Provider);
			table.Create(TestDb.Provider);

			Assert.IsTrue(table.Exists(TestDb.Provider));
		}

		[TestMethod]
		public void DropSimpleTable()
		{
			var table = _simpleDataModel.Fields.First().Storage.Table;

			if (!table.Exists(TestDb.Provider))
				table.Create(TestDb.Provider);
			table.Drop(TestDb.Provider);

			Assert.IsFalse(table.Exists(TestDb.Provider));
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
