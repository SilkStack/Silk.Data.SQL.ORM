using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class ModellingTests
	{
		[TestMethod]
		public void BasicSqlTypes()
		{
			var domain = new DataDomain();
			var dataModel = domain.CreateDataModel<BasicSqlTypesModel,BasicSqlTypesModel>();

			Assert.AreEqual(11, dataModel.Fields.Length);
			foreach (var field in dataModel.Fields)
			{
				Assert.IsTrue(field.Storage.Table.DataFields.Contains(field));
				switch (field.Name)
				{
					case "Bit":
						Assert.AreEqual(SqlBaseType.Bit, field.Storage.DataType.BaseType);
						break;
					case "TinyInt":
						Assert.AreEqual(SqlBaseType.TinyInt, field.Storage.DataType.BaseType);
						break;
					case "SmallInt":
						Assert.AreEqual(SqlBaseType.SmallInt, field.Storage.DataType.BaseType);
						break;
					case "Int":
						Assert.AreEqual(SqlBaseType.Int, field.Storage.DataType.BaseType);
						break;
					case "BigInt":
						Assert.AreEqual(SqlBaseType.BigInt, field.Storage.DataType.BaseType);
						break;
					case "Float":
						Assert.AreEqual(SqlBaseType.Float, field.Storage.DataType.BaseType);
						Assert.AreEqual(SqlDataType.FLOAT_MAX_PRECISION, field.Storage.DataType.Parameters[0]);
						break;
					case "Double":
						Assert.AreEqual(SqlBaseType.Float, field.Storage.DataType.BaseType);
						Assert.AreEqual(SqlDataType.DOUBLE_MAX_PRECISION, field.Storage.DataType.Parameters[0]);
						break;
					case "Decimal":
						Assert.AreEqual(SqlBaseType.Decimal, field.Storage.DataType.BaseType);
						break;
					case "Text":
						Assert.AreEqual(SqlBaseType.Text, field.Storage.DataType.BaseType);
						break;
					case "Guid":
						Assert.AreEqual(SqlBaseType.Guid, field.Storage.DataType.BaseType);
						break;
					case "DateTime":
						Assert.AreEqual(SqlBaseType.DateTime, field.Storage.DataType.BaseType);
						break;
				}
			}
		}

		[TestMethod]
		public void BasicSqlTypesWithoutView()
		{
			var domain = new DataDomain();
			var dataModel = domain.CreateDataModel<BasicSqlTypesModel>();

			Assert.AreEqual(11, dataModel.Fields.Length);
			foreach (var field in dataModel.Fields)
			{
				Assert.IsTrue(field.Storage.Table.DataFields.Contains(field));
				switch (field.Name)
				{
					case "Bit":
						Assert.AreEqual(SqlBaseType.Bit, field.Storage.DataType.BaseType);
						break;
					case "TinyInt":
						Assert.AreEqual(SqlBaseType.TinyInt, field.Storage.DataType.BaseType);
						break;
					case "SmallInt":
						Assert.AreEqual(SqlBaseType.SmallInt, field.Storage.DataType.BaseType);
						break;
					case "Int":
						Assert.AreEqual(SqlBaseType.Int, field.Storage.DataType.BaseType);
						break;
					case "BigInt":
						Assert.AreEqual(SqlBaseType.BigInt, field.Storage.DataType.BaseType);
						break;
					case "Float":
						Assert.AreEqual(SqlBaseType.Float, field.Storage.DataType.BaseType);
						Assert.AreEqual(SqlDataType.FLOAT_MAX_PRECISION, field.Storage.DataType.Parameters[0]);
						break;
					case "Double":
						Assert.AreEqual(SqlBaseType.Float, field.Storage.DataType.BaseType);
						Assert.AreEqual(SqlDataType.DOUBLE_MAX_PRECISION, field.Storage.DataType.Parameters[0]);
						break;
					case "Decimal":
						Assert.AreEqual(SqlBaseType.Decimal, field.Storage.DataType.BaseType);
						break;
					case "Text":
						Assert.AreEqual(SqlBaseType.Text, field.Storage.DataType.BaseType);
						break;
					case "Guid":
						Assert.AreEqual(SqlBaseType.Guid, field.Storage.DataType.BaseType);
						break;
					case "DateTime":
						Assert.AreEqual(SqlBaseType.DateTime, field.Storage.DataType.BaseType);
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
