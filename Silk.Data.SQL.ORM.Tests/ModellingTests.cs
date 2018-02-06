using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class ModellingTests
	{
		[TestMethod]
		public void BasicSqlTypes()
		{
			var dataModel = TestDb.CreateDomainAndSchema<BasicSqlTypesModel>();

			Assert.AreEqual(11, dataModel.Fields.Length);
			foreach (var field in dataModel.Fields)
			{
				switch (field.Name)
				{
					case "Bit":
						Assert.AreEqual(SqlBaseType.Bit, field.SqlType.BaseType);
						break;
					case "TinyInt":
						Assert.AreEqual(SqlBaseType.TinyInt, field.SqlType.BaseType);
						break;
					case "SmallInt":
						Assert.AreEqual(SqlBaseType.SmallInt, field.SqlType.BaseType);
						break;
					case "Int":
						Assert.AreEqual(SqlBaseType.Int, field.SqlType.BaseType);
						break;
					case "BigInt":
						Assert.AreEqual(SqlBaseType.BigInt, field.SqlType.BaseType);
						break;
					case "Float":
						Assert.AreEqual(SqlBaseType.Float, field.SqlType.BaseType);
						Assert.AreEqual(SqlDataType.FLOAT_MAX_PRECISION, field.SqlType.Parameters[0]);
						break;
					case "Double":
						Assert.AreEqual(SqlBaseType.Float, field.SqlType.BaseType);
						Assert.AreEqual(SqlDataType.DOUBLE_MAX_PRECISION, field.SqlType.Parameters[0]);
						break;
					case "Decimal":
						Assert.AreEqual(SqlBaseType.Decimal, field.SqlType.BaseType);
						break;
					case "Text":
						Assert.AreEqual(SqlBaseType.Text, field.SqlType.BaseType);
						break;
					case "Guid":
						Assert.AreEqual(SqlBaseType.Guid, field.SqlType.BaseType);
						break;
					case "DateTime":
						Assert.AreEqual(SqlBaseType.DateTime, field.SqlType.BaseType);
						break;
				}
			}
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
