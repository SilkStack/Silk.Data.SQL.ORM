using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.Modelling;
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

			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Bool) && field.Column.SqlDataType.BaseType == SqlBaseType.Bit
				));
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Byte) && field.Column.SqlDataType.BaseType == SqlBaseType.TinyInt && field.Column.SqlDataType.Unsigned
				));
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.SByte) && field.Column.SqlDataType.BaseType == SqlBaseType.TinyInt && !field.Column.SqlDataType.Unsigned
				));
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.UShort) && field.Column.SqlDataType.BaseType == SqlBaseType.SmallInt && field.Column.SqlDataType.Unsigned
				));
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Short) && field.Column.SqlDataType.BaseType == SqlBaseType.SmallInt && !field.Column.SqlDataType.Unsigned
				));
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.UInt) && field.Column.SqlDataType.BaseType == SqlBaseType.Int && field.Column.SqlDataType.Unsigned
				));
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Int) && field.Column.SqlDataType.BaseType == SqlBaseType.Int && !field.Column.SqlDataType.Unsigned
				));
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.ULong) && field.Column.SqlDataType.BaseType == SqlBaseType.BigInt && field.Column.SqlDataType.Unsigned
				));
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Long) && field.Column.SqlDataType.BaseType == SqlBaseType.BigInt && !field.Column.SqlDataType.Unsigned
				));
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Float) && field.Column.SqlDataType.BaseType == SqlBaseType.Float
				));
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Double) && field.Column.SqlDataType.BaseType == SqlBaseType.Float
				));
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Decimal) && field.Column.SqlDataType.BaseType == SqlBaseType.Decimal
				));
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.DateTime) && field.Column.SqlDataType.BaseType == SqlBaseType.DateTime
				));
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Guid) && field.Column.SqlDataType.BaseType == SqlBaseType.Guid
				));
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.String) && field.Column.SqlDataType.BaseType == SqlBaseType.Text
				));
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(field =>
				field.FieldName == nameof(PrimitiveSQLTypes.Enum) && field.Column.SqlDataType.BaseType == SqlBaseType.Int
				));
		}

		[TestMethod]
		public void EmbedPocoTypeIntoSchema()
		{
			var builder = new SchemaBuilder();
			builder.DefineEntity<ClassWithEmbeddedPoco>();
			builder.DefineEntity<HasIntId>();
			var schema = builder.Build();
			var model = schema.GetEntityModel<ClassWithEmbeddedPoco>();

			Assert.AreEqual(2, model.Fields.Length);
			Assert.AreEqual(6, model.EntityTable.Columns.Length);
			var tableColumns = model.EntityTable.Columns;

			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(q => q.FieldName == "Data"));
			var embeddedField = model.Fields.OfType<IEmbeddedObjectField>().FirstOrDefault();
			Assert.IsNotNull(embeddedField);
			Assert.AreEqual("Embedded", embeddedField.FieldName);
			Assert.AreEqual(2, embeddedField.EmbeddedFields.Length);
			Assert.IsTrue(tableColumns.Any(q => q.ColumnName == "Data" && q.SqlDataType.BaseType == SqlBaseType.Text));
			Assert.IsTrue(tableColumns.Any(q => q.ColumnName == "Embedded" && q.SqlDataType.BaseType == SqlBaseType.Bit));

			Assert.IsTrue(embeddedField.EmbeddedFields.OfType<IValueField>().Any(q => q.FieldName == "Data"));
			embeddedField = embeddedField.EmbeddedFields.OfType<IEmbeddedObjectField>().FirstOrDefault();
			Assert.IsNotNull(embeddedField);
			Assert.AreEqual("SubEmbedded", embeddedField.FieldName);
			Assert.AreEqual(2, embeddedField.EmbeddedFields.Length);
			Assert.IsTrue(tableColumns.Any(q => q.ColumnName == "Embedded_Data" && q.SqlDataType.BaseType == SqlBaseType.Text));
			Assert.IsTrue(tableColumns.Any(q => q.ColumnName == "Embedded_SubEmbedded" && q.SqlDataType.BaseType == SqlBaseType.Bit));

			Assert.IsTrue(embeddedField.EmbeddedFields.OfType<IValueField>().Any(q => q.FieldName == "Data"));
			var singleRelatedObjectField = embeddedField.EmbeddedFields.OfType<ISingleRelatedObjectField>().FirstOrDefault();
			var dataField = embeddedField.EmbeddedFields.OfType<IValueField>().FirstOrDefault();
			Assert.IsNotNull(singleRelatedObjectField);
			Assert.IsNotNull(dataField);
			Assert.AreEqual("Relationship", singleRelatedObjectField.FieldName);
			Assert.AreEqual("Data", dataField.FieldName);
			Assert.IsTrue(tableColumns.Any(q => q.ColumnName == "Embedded_SubEmbedded_Data" && q.SqlDataType.BaseType == SqlBaseType.Text));
			Assert.IsTrue(tableColumns.Any(q => q.ColumnName == "Embedded_SubEmbedded_Relationship" && q.SqlDataType.BaseType == SqlBaseType.Int));
		}

		[TestMethod]
		public void ModelSingleObjectRelationship()
		{
			var builder = new SchemaBuilder();
			builder.DefineEntity<HasSingleRelationship>();
			builder.DefineEntity<HasIntId>();
			var schema = builder.Build();
			var model = schema.GetEntityModel<HasSingleRelationship>();
			var relatedModel = schema.GetEntityModel<HasIntId>();
			var primaryKeyField = relatedModel.Fields.OfType<IValueField>().First(q => q.Column.IsPrimaryKey);

			Assert.AreEqual(1, model.Fields.Length);
			Assert.AreEqual(1, model.EntityTable.Columns.Length);
			Assert.AreEqual(primaryKeyField.Column.SqlDataType.BaseType, model.EntityTable.Columns[0].SqlDataType.BaseType);
			var field = model.Fields.First();
			Assert.IsInstanceOfType(field, typeof(ISingleRelatedObjectField));

			var relatedObjectField = field as ISingleRelatedObjectField;
			Assert.ReferenceEquals(relatedModel, relatedObjectField.RelatedObjectModel);
			Assert.ReferenceEquals(primaryKeyField, relatedObjectField.RelatedPrimaryKey);
		}

		[TestMethod]
		public void ModelMultipleObjectRelationship()
		{
			var builder = new SchemaBuilder();
			builder.DefineEntity<HasManyPrimitives>();
			builder.DefineEntity<HasIntId>();
			var schema = builder.Build();
			var model = schema.GetEntityModel<HasManyPrimitives>();

			Assert.AreEqual(2, model.Fields.Length);
			var field = model.Fields.OfType<IManyRelatedObjectField>().FirstOrDefault();
			Assert.IsInstanceOfType(field, typeof(ManyRelatedObjectField<List<HasIntId>, HasIntId, int>));
			Assert.IsNotNull(field);
			Assert.AreEqual("Relationships", field.FieldName);
			Assert.AreEqual(typeof(HasIntId), field.ElementType);
			Assert.ReferenceEquals(TypeModel.GetModelOf<HasIntId>(), field.ElementModel);
			Assert.IsNotNull(field.JunctionTable);
			Assert.IsNotNull(field.LocalColumn);
			Assert.IsNotNull(field.LocalJunctionColumn);
			Assert.IsNotNull(field.LocalIdentifierField);
			Assert.IsNotNull(field.Mapping);
			Assert.IsNotNull(field.RelatedJunctionColumn);
			Assert.IsNotNull(field.RelatedObjectModel);
			Assert.ReferenceEquals(TypeModel.GetModelOf<HasIntId>(), field.RelatedObjectModel);
			Assert.IsNotNull(field.RelatedPrimaryKey);
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
			var field = model.Fields.OfType<IValueField>().First();
			Assert.IsTrue(field.Column.IsPrimaryKey);
			Assert.IsTrue(field.Column.IsAutoIncrement);

			model = schema.GetEntityModel<HasIntId>();
			field = model.Fields.OfType<IValueField>().First();
			Assert.IsTrue(field.Column.IsPrimaryKey);
			Assert.IsTrue(field.Column.IsAutoIncrement);

			model = schema.GetEntityModel<HasLongId>();
			field = model.Fields.OfType<IValueField>().First();
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
			var field = model.Fields.OfType<IValueField>().First();
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

		private class HasSingleRelationship
		{
			public HasIntId Relationship { get; set; }
		}

		private class HasManyPrimitives
		{
			public int Id { get; set; }
			public List<HasIntId> Relationships { get; set; }
				= new List<HasIntId>();
		}

		private class ClassWithEmbeddedPoco
		{
			public EmbeddedPocoTier1 Embedded { get; set; }
			public string Data { get; set; }
		}

		private class EmbeddedPocoTier1
		{
			public EmbeddedPocoTier2 SubEmbedded { get; set; }
			public string Data { get; set; }
		}

		private class EmbeddedPocoTier2
		{
			public HasIntId Relationship { get; set; }
			public string Data { get; set; }
		}
	}
}
