using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class ModellingTests
	{
		[TestMethod]
		public void ModelSqlPrimitiveTypes()
		{
			var builder = new SchemaBuilder();
			builder.DefineEntity<PrimitiveSQLTypes>();
			var schema = builder.Build();
			var model = schema.GetEntityModel<PrimitiveSQLTypes>();

			Assert.IsNotNull(model);
			Assert.AreEqual(typeof(PrimitiveSQLTypes), model.EntityType);
			Assert.AreEqual(16, model.Fields.Length);
			Assert.IsNotNull(model.EntityTable);
			Assert.AreEqual(16, model.EntityTable.Columns.Length);

			Assert.IsTrue(model.Fields.OfType<ValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Bool) && field.Column.SqlDataType.BaseType == SqlBaseType.Bit
				));
			Assert.IsTrue(model.Fields.OfType<ValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Byte) && field.Column.SqlDataType.BaseType == SqlBaseType.TinyInt && field.Column.SqlDataType.Unsigned
				));
			Assert.IsTrue(model.Fields.OfType<ValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.SByte) && field.Column.SqlDataType.BaseType == SqlBaseType.TinyInt && !field.Column.SqlDataType.Unsigned
				));
			Assert.IsTrue(model.Fields.OfType<ValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.UShort) && field.Column.SqlDataType.BaseType == SqlBaseType.SmallInt && field.Column.SqlDataType.Unsigned
				));
			Assert.IsTrue(model.Fields.OfType<ValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Short) && field.Column.SqlDataType.BaseType == SqlBaseType.SmallInt && !field.Column.SqlDataType.Unsigned
				));
			Assert.IsTrue(model.Fields.OfType<ValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.UInt) && field.Column.SqlDataType.BaseType == SqlBaseType.Int && field.Column.SqlDataType.Unsigned
				));
			Assert.IsTrue(model.Fields.OfType<ValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Int) && field.Column.SqlDataType.BaseType == SqlBaseType.Int && !field.Column.SqlDataType.Unsigned
				));
			Assert.IsTrue(model.Fields.OfType<ValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.ULong) && field.Column.SqlDataType.BaseType == SqlBaseType.BigInt && field.Column.SqlDataType.Unsigned
				));
			Assert.IsTrue(model.Fields.OfType<ValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Long) && field.Column.SqlDataType.BaseType == SqlBaseType.BigInt && !field.Column.SqlDataType.Unsigned
				));
			Assert.IsTrue(model.Fields.OfType<ValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Float) && field.Column.SqlDataType.BaseType == SqlBaseType.Float
				));
			Assert.IsTrue(model.Fields.OfType<ValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Double) && field.Column.SqlDataType.BaseType == SqlBaseType.Float
				));
			Assert.IsTrue(model.Fields.OfType<ValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Decimal) && field.Column.SqlDataType.BaseType == SqlBaseType.Decimal
				));
			Assert.IsTrue(model.Fields.OfType<ValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.DateTime) && field.Column.SqlDataType.BaseType == SqlBaseType.DateTime
				));
			Assert.IsTrue(model.Fields.OfType<ValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Guid) && field.Column.SqlDataType.BaseType == SqlBaseType.Guid
				));
			Assert.IsTrue(model.Fields.OfType<ValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.String) && field.Column.SqlDataType.BaseType == SqlBaseType.Text
				));
			Assert.IsTrue(model.Fields.OfType<ValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Enum) && field.Column.SqlDataType.BaseType == SqlBaseType.Int
				));
		}

		[TestMethod]
		public void EmbedPocoTypeIntoSchema()
		{
			throw new NotImplementedException();
		}

		[TestMethod]
		public void ModelSingleObjectRelationship()
		{
			throw new NotImplementedException();
		}

		[TestMethod]
		public void ModelMultipleObjectRelationship()
		{
			var builder = new SchemaBuilder();
			builder.DefineEntity<HasManyPrimitives>();
			builder.DefineEntity<HasIntId>();
			var schema = builder.Build();
			throw new NotImplementedException();
		}

		[TestMethod]
		public void ModelIntegerIdAsAutoIncrementPrimaryKey()
		{
			var builder = new SchemaBuilder();
			builder.DefineEntity<HasShortId>();
			builder.DefineEntity<HasIntId>();
			builder.DefineEntity<HasLongId>();
			var schema = builder.Build();
			EntityModel model = schema.GetEntityModel<HasShortId>();
			var field = model.Fields.OfType<ValueField>().First();
			Assert.IsTrue(field.Column.IsPrimaryKey);
			Assert.IsTrue(field.Column.IsAutoIncrement);

			model = schema.GetEntityModel<HasIntId>();
			field = model.Fields.OfType<ValueField>().First();
			Assert.IsTrue(field.Column.IsPrimaryKey);
			Assert.IsTrue(field.Column.IsAutoIncrement);

			model = schema.GetEntityModel<HasLongId>();
			field = model.Fields.OfType<ValueField>().First();
			Assert.IsTrue(field.Column.IsPrimaryKey);
			Assert.IsTrue(field.Column.IsAutoIncrement);
		}

		[TestMethod]
		public void ModelGuidIdAsGeneratedPrimaryKey()
		{
			var builder = new SchemaBuilder();
			builder.DefineEntity<HasGuidId>();
			var schema = builder.Build();
			var model = schema.GetEntityModel<HasGuidId>();
			var field = model.Fields.OfType<ValueField>().First();
			Assert.IsTrue(field.Column.IsPrimaryKey);
			Assert.IsTrue(field.Column.IsAutoGenerated);
		}

		private class PrimitiveSQLTypes
		{
			public bool Bool { get; set; }
			public byte Byte { get; set; }
			public sbyte SByte { get; set; }
			public short Short { get; set; }
			public ushort UShort { get; set; }
			public int Int { get; set; }
			public uint UInt { get; set; }
			public long Long { get; set; }
			public ulong ULong { get; set; }
			public float Float { get; set; }
			public double Double { get; set; }
			public decimal Decimal { get; set; }
			public DateTime DateTime { get; set; }
			public Guid Guid { get; set; }
			public string String { get; set; }
			public Enum Enum { get; set; }
		}

		private class HasShortId
		{
			public short Id { get; set; }
		}

		private class HasIntId
		{
			public int Id { get; set; }
		}

		private class HasLongId
		{
			public long Id { get; set; }
		}

		private class HasGuidId
		{
			public Guid Id { get; set; }
		}

		private enum Enum
		{
			Value1,
			Value2
		}

		private class HasManyPrimitives
		{
			public List<HasIntId> Relationships { get; set; }
				= new List<HasIntId>();
		}
	}
}
