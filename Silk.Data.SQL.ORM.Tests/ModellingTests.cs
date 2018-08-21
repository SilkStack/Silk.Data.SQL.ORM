using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		public void ModelPrimitivePoco()
		{
			var expectedFields = new (Type type, string Name, SqlDataType DataType)[]
			{
				(typeof(bool), "Bool", SqlDataType.Bit()),
				(typeof(sbyte), "SByte", SqlDataType.TinyInt()),
				(typeof(byte), "Byte", SqlDataType.UnsignedTinyInt()),
				(typeof(ushort), "UShort", SqlDataType.UnsignedSmallInt()),
				(typeof(short), "Short", SqlDataType.SmallInt()),
				(typeof(uint), "UInt", SqlDataType.UnsignedInt()),
				(typeof(int), "Int", SqlDataType.Int()),
				(typeof(ulong), "ULong", SqlDataType.UnsignedBigInt()),
				(typeof(long), "Long", SqlDataType.BigInt()),
				(typeof(float), "Float", SqlDataType.Float(SqlDataType.FLOAT_MAX_PRECISION)),
				(typeof(double), "Double", SqlDataType.Float(SqlDataType.DOUBLE_MAX_PRECISION)),
				(typeof(decimal), "Decimal", SqlDataType.Decimal()),
				(typeof(DateTime), "DateTime", SqlDataType.DateTime()),
				(typeof(Guid), "Guid", SqlDataType.Guid()),
				(typeof(string), "String", SqlDataType.Text())
			};

			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<Primitives>();

			var schema = schemaBuilder.Build();
			var entitySchema = schema.GetEntitySchema<Primitives>();

			Assert.IsNotNull(entitySchema);
			Assert.AreEqual(expectedFields.Length, entitySchema.EntityFields.Length);
			Assert.AreEqual(expectedFields.Length, entitySchema.EntityTable.Columns.Length);
			Assert.AreEqual(expectedFields.Length, entitySchema.ProjectionFields.Length);

			foreach (var (type, name, dataType) in expectedFields)
			{
				var entityField = entitySchema.EntityFields.FirstOrDefault(q => q.FieldType == type && q.ModelField.FieldName == name);
				if (entityField == null)
					Assert.Fail("Expected entity field not present on entity schema.");
				var column = entitySchema.EntityTable.Columns.FirstOrDefault(q => q.ColumnName == name &&
					TypesAreEqual(dataType, q.DataType));
				if (column == null)
					Assert.Fail("Expected column not present in entity table.");
				var projectionField = entitySchema.ProjectionFields.FirstOrDefault(q =>
					q.SourceName == entitySchema.EntityTable.TableName &&
					q.FieldName == column.ColumnName &&
					q.AliasName == entityField.ModelField.FieldName
					);
				if (projectionField == null)
					Assert.Fail("Expected projection field not present on entity schema.");
			}
		}

		private static bool TypesAreEqual(SqlDataType one, SqlDataType two)
		{
			if (one.BaseType != two.BaseType)
				return false;
			if (one.Unsigned != two.Unsigned)
				return false;
			if (one.Parameters == null && two.Parameters != null)
				return false;
			if (one.Parameters != null && two.Parameters == null)
				return false;
			if (one.Parameters != null && two.Parameters != null && !one.Parameters.SequenceEqual(two.Parameters))
				return false;
			return true;
		}

		private class Primitives
		{
			public bool Bool { get; set; }
			public sbyte SByte { get; set; }
			public byte Byte { get; set; }
			public ushort UShort { get; set; }
			public short Short { get; set; }
			public uint UInt { get; set; }
			public int Int { get; set; }
			public ulong ULong { get; set; }
			public long Long { get; set; }
			public string String { get; set; }
			public DateTime DateTime { get; set; }
			public Guid Guid { get; set; }
			public float Float { get; set; }
			public double Double { get; set; }
			public decimal Decimal { get; set; }
		}
	}
}
