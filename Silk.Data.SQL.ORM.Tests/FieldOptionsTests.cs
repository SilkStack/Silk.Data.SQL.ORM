using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Schema;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class FieldOptionsTests
	{
		[TestMethod]
		public void ChangeTableName()
		{
			var schemaBuilder = new SchemaBuilder();
			var entityOptions = schemaBuilder.DefineEntity<SingleFieldPoco<int>>();
			entityOptions.TableName("CustomTable");
			var schema = schemaBuilder.Build();

			var model = schema.GetEntityModel<SingleFieldPoco<int>>();
			Assert.AreEqual("CustomTable", model.EntityTable.TableName);
		}

		[TestMethod]
		public void ChangeColumnName()
		{
			var schemaBuilder = new SchemaBuilder();
			var entityOptions = schemaBuilder.DefineEntity<SingleFieldPoco<int>>();
			entityOptions.For(q => q.Field).ColumnName("Custom");
			var schema = schemaBuilder.Build();

			var model = schema.GetEntityModel<SingleFieldPoco<int>>();
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(q => q.FieldName == nameof(SingleFieldPoco<int>.Field) && q.Column.ColumnName == "Custom"));
		}

		[TestMethod]
		public void CustomPrimaryKey()
		{
			var schemaBuilder = new SchemaBuilder();
			var entityOptions = schemaBuilder.DefineEntity<SingleFieldPoco<int>>();
			entityOptions.For(q => q.Field).PrimaryKey(autoGenerate: true);
			var schema = schemaBuilder.Build();

			var model = schema.GetEntityModel<SingleFieldPoco<int>>();
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(q =>
				q.FieldName == nameof(SingleFieldPoco<int>.Field) &&
				q.Column.IsPrimaryKey && q.Column.IsServerGenerated));
		}

		[TestMethod]
		public void CustomStringLength()
		{
			var schemaBuilder = new SchemaBuilder();
			var entityOptions = schemaBuilder.DefineEntity<SingleFieldPoco<string>>();
			entityOptions.For(q => q.Field).Length(255);
			var schema = schemaBuilder.Build();

			var model = schema.GetEntityModel<SingleFieldPoco<string>>();
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(q =>
				q.FieldName == nameof(SingleFieldPoco<string>.Field) &&
				q.Column.SqlDataType.BaseType == SqlBaseType.Text &&
				q.Column.SqlDataType.Parameters.FirstOrDefault() == 255));
		}

		[TestMethod]
		public void CustomPrecisionOnFloat()
		{
			var schemaBuilder = new SchemaBuilder();
			var entityOptions = schemaBuilder.DefineEntity<SingleFieldPoco<float>>();
			entityOptions.For(q => q.Field).Precision(20);
			var schema = schemaBuilder.Build();

			var model = schema.GetEntityModel<SingleFieldPoco<float>>();
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(q =>
				q.FieldName == nameof(SingleFieldPoco<float>.Field) &&
				q.Column.SqlDataType.BaseType == SqlBaseType.Float &&
				q.Column.SqlDataType.Parameters.FirstOrDefault() == 20));
		}

		[TestMethod]
		public void CustomPrecisionOnDouble()
		{
			var schemaBuilder = new SchemaBuilder();
			var entityOptions = schemaBuilder.DefineEntity<SingleFieldPoco<double>>();
			entityOptions.For(q => q.Field).Precision(40);
			var schema = schemaBuilder.Build();

			var model = schema.GetEntityModel<SingleFieldPoco<double>>();
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(q =>
				q.FieldName == nameof(SingleFieldPoco<double>.Field) &&
				q.Column.SqlDataType.BaseType == SqlBaseType.Float &&
				q.Column.SqlDataType.Parameters.FirstOrDefault() == 40));
		}

		[TestMethod]
		public void CustomPrecisionOnDecimal()
		{
			var schemaBuilder = new SchemaBuilder();
			var entityOptions = schemaBuilder.DefineEntity<SingleFieldPoco<decimal>>();
			entityOptions.For(q => q.Field).Precision(20, 40);
			var schema = schemaBuilder.Build();

			var model = schema.GetEntityModel<SingleFieldPoco<decimal>>();
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(q =>
				q.FieldName == nameof(SingleFieldPoco<decimal>.Field) &&
				q.Column.SqlDataType.BaseType == SqlBaseType.Decimal &&
				q.Column.SqlDataType.Parameters[0] == 20 &&
				q.Column.SqlDataType.Parameters[1] == 40));
		}

		[TestMethod]
		public void CustomTypeOnDecimal()
		{
			var schemaBuilder = new SchemaBuilder();
			var entityOptions = schemaBuilder.DefineEntity<SingleFieldPoco<decimal>>();
			entityOptions.For(q => q.Field).DataType(SqlDataType.Decimal(20, 40));
			var schema = schemaBuilder.Build();

			var model = schema.GetEntityModel<SingleFieldPoco<decimal>>();
			Assert.IsTrue(model.Fields.OfType<IValueField>().Any(q =>
				q.FieldName == nameof(SingleFieldPoco<decimal>.Field) &&
				q.Column.SqlDataType.BaseType == SqlBaseType.Decimal &&
				q.Column.SqlDataType.Parameters[0] == 20 &&
				q.Column.SqlDataType.Parameters[1] == 40));
		}

		[TestMethod]
		public void CustomizeEmbeddedType()
		{
			var schemaBuilder = new SchemaBuilder();
			var entityOptions = schemaBuilder.DefineEntity<SingleFieldPoco<EmbeddedFieldPoco>>();
			entityOptions.For(q => q.Field.EmbeddedField).ColumnName("Custom");
			var schema = schemaBuilder.Build();

			var model = schema.GetEntityModel<SingleFieldPoco<EmbeddedFieldPoco>>();
			var embeddedField = model.Fields.OfType<IEmbeddedObjectField>().FirstOrDefault();
			Assert.IsTrue(embeddedField.EmbeddedFields.OfType<IValueField>().Any(q => q.FieldName == "EmbeddedField" && q.Column.ColumnName == "Custom"));
		}

		private class SingleFieldPoco<T>
		{
			public T Field { get; set; }
		}

		private class EmbeddedFieldPoco
		{
			public int EmbeddedField { get; set; }
		}
	}
}
