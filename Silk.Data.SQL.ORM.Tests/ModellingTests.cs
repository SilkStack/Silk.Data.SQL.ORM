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
				var entityField = entitySchema.EntityFields.FirstOrDefault(q => q.DataType == type && q.ModelField.FieldName == name);
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
				Assert.IsTrue(projectionField.ModelPath.SequenceEqual(new[] { name }),
					"Projection field model path is invalid.");
			}
		}

		[TestMethod]
		public void ModelPrimaryKeyByConvention()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<ConventionPrimaryKey>();

			var schema = schemaBuilder.Build();
			var entitySchema = schema.GetEntitySchema<ConventionPrimaryKey>();

			var idField = entitySchema.EntityFields.First(q => q.ModelField.FieldName == nameof(ConventionPrimaryKey.Id));
			Assert.IsTrue(idField.IsPrimaryKey, "ID field is not a primary key.");
		}

		[TestMethod]
		public void ModelCustomPrimaryKey()
		{
			var schemaBuilder = new SchemaBuilder();
			var entityBuilder = schemaBuilder.DefineEntity<CustomPrimaryKey>();
			entityBuilder.For(q => q.Id).IsPrimaryKey = false;
			entityBuilder.For(q => q.PrimaryKey).IsPrimaryKey = true;

			var schema = schemaBuilder.Build();
			var entitySchema = schema.GetEntitySchema<CustomPrimaryKey>();

			var idField = entitySchema.EntityFields.First(q => q.ModelField.FieldName == nameof(CustomPrimaryKey.Id));
			Assert.IsFalse(idField.IsPrimaryKey, "ID field shouldn't be a primary key.");

			var primaryKeyField = entitySchema.EntityFields.First(q => q.ModelField.FieldName == nameof(CustomPrimaryKey.PrimaryKey));
			Assert.IsTrue(primaryKeyField.IsPrimaryKey, "Primary key field is not a primary key.");
		}

		[TestMethod]
		public void ModelManyToOneRelationship()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<Parent>();
			schemaBuilder.DefineEntity<Child>();

			var schema = schemaBuilder.Build();
			var parentSchema = schema.GetEntitySchema<Parent>();

			Assert.IsNotNull(parentSchema);
			var entityField = parentSchema.EntityFields.FirstOrDefault(q => q.ModelField.FieldName == nameof(Parent.Child));
			if (entityField == null)
				Assert.Fail("Child entity field not present on entity schema.");
			var foreignKeyColumn = parentSchema.EntityTable.Columns.FirstOrDefault(q => q.ColumnName == "FK_Child_Id");
			if (foreignKeyColumn == null)
				Assert.Fail("Foreign key column not present in entity table.");
			var projectionField = parentSchema.ProjectionFields.FirstOrDefault(q => q.AliasName == "Child_Id");
			if (projectionField == null)
				Assert.Fail("Child Id field not present in projection.");
			Assert.IsTrue(projectionField.ModelPath.SequenceEqual(new[] { "Child", "Id" }), "Child Id model path is incorrect.");
			projectionField = parentSchema.ProjectionFields.FirstOrDefault(q => q.AliasName == "Child_Data");
			if (projectionField == null)
				Assert.Fail("Child Data field not present in projection.");
			Assert.IsTrue(projectionField.ModelPath.SequenceEqual(new[] { "Child", "Data" }), "Child Data model path is incorrect.");
			var join = parentSchema.EntityJoins.FirstOrDefault(q => q.TableName == "Child");
			if (join == null)
				Assert.Fail("Join to child table not present.");
		}

		[TestMethod]
		public void ModelDeepManyToOneRelationship()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<Child>();
			schemaBuilder.DefineEntity<Parent>();
			schemaBuilder.DefineEntity<DeepParent>();

			var schema = schemaBuilder.Build();
			var parentSchema = schema.GetEntitySchema<DeepParent>();

			var entityField = parentSchema.EntityFields.FirstOrDefault(q => q.ModelField.FieldName == nameof(DeepParent.Child));
			if (entityField == null)
				Assert.Fail("Child entity field not present on entity schema.");
			var foreignKeyColumn = parentSchema.EntityTable.Columns.FirstOrDefault(q => q.ColumnName == "FK_Child_Id");
			if (foreignKeyColumn == null)
				Assert.Fail("Foreign key column not present in entity table.");
			var projectionField = parentSchema.ProjectionFields.FirstOrDefault(q => q.AliasName == "Child_Id");
			if (projectionField == null)
				Assert.Fail("Child Id field not present in projection.");
			Assert.IsTrue(projectionField.ModelPath.SequenceEqual(new[] { "Child", "Id" }), "Child Id model path is incorrect.");
			projectionField = parentSchema.ProjectionFields.FirstOrDefault(q => q.AliasName == "Child_Child_Id");
			if (projectionField == null)
				Assert.Fail("Deep child Id field not present in projection.");
			Assert.IsTrue(projectionField.ModelPath.SequenceEqual(new[] { "Child", "Child", "Id" }), "Deep child Id model path is incorrect.");
			projectionField = parentSchema.ProjectionFields.FirstOrDefault(q => q.AliasName == "Child_Child_Data");
			if (projectionField == null)
				Assert.Fail("Deep child Data field not present in projection.");
			Assert.IsTrue(projectionField.ModelPath.SequenceEqual(new[] { "Child", "Child", "Data" }), "Deep child Data model path is incorrect.");

			var join = parentSchema.EntityJoins.FirstOrDefault(q => q.TableName == "Parent");
			if (join == null)
				Assert.Fail("Join to parent table not present.");
			join = parentSchema.EntityJoins.FirstOrDefault(q => q.TableName == "Child");
			if (join == null)
				Assert.Fail("Join to child table not present.");
		}

		[TestMethod]
		public void ModelNestedEmbeddedObjects()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<DeepParent>();

			var schema = schemaBuilder.Build();
			var entitySchema = schema.GetEntitySchema<DeepParent>();

			var entityField = entitySchema.EntityFields.FirstOrDefault(q => q.ModelField.FieldName == nameof(DeepParent.Child));
			if (entityField == null)
				Assert.Fail("Child entity field not present on entity schema.");

			var expectedProjections = new (string FieldName, string Alias, string Source, string[] Path)[]
			{
				("Child_Id", "Child_Id", "DeepParent", new[]{ "Child", "Id" }),
				("Child_Child_Id", "Child_Child_Id", "DeepParent", new[]{ "Child", "Child", "Id" }),
				("Child_Child_Data", "Child_Child_Data", "DeepParent", new[]{ "Child", "Child", "Data" })
			};
			CheckForProjections(entitySchema, expectedProjections);

			//  all columns are in table
			var expectedColumns = new (string FieldName, SqlBaseType SqlBaseType)[]
			{
				("Child", SqlBaseType.Bit), //  Child property null check
				("Child_Id", SqlBaseType.Int),
				("Child_Child", SqlBaseType.Bit), //  Child.Child property null check
				("Child_Child_Id", SqlBaseType.Int),
				("Child_Child_Data", SqlBaseType.Text)
			};
			CheckForColumns(entitySchema, expectedColumns);
		}

		[TestMethod]
		public void ModelEmbeddedObjectWithRelationship()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<DeepParent>();
			schemaBuilder.DefineEntity<Child>();

			var schema = schemaBuilder.Build();
			var entitySchema = schema.GetEntitySchema<DeepParent>();

			var expectedProjections = new(string FieldName, string Alias, string Source, string[] Path)[]
			{
				("Child_Id", "Child_Id", "DeepParent", new[]{ "Child", "Id" }),
				("Id", "Child_Child_Id", "__joinAlias_Child_Child", new[]{ "Child", "Child", "Id" }),
				("Data", "Child_Child_Data", "__joinAlias_Child_Child", new[]{ "Child", "Child", "Data" })
			};
			CheckForProjections(entitySchema, expectedProjections);

			//  all columns are in table
			var expectedColumns = new(string FieldName, SqlBaseType SqlBaseType)[]
			{
				("Child", SqlBaseType.Bit),			//  Child property null check
				("Child_Id", SqlBaseType.Int),		//  Embedded Child.Id
				("FK_Child_Id", SqlBaseType.Int)	//  Child.Child property null check
			};
			CheckForColumns(entitySchema, expectedColumns);
		}

		[TestMethod]
		public void ModelRelationshipWithEmbeddedObjects()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<DeepParent>();
			schemaBuilder.DefineEntity<Parent>().For(q => q.Child.Id).IsPrimaryKey = false;

			var schema = schemaBuilder.Build();
			var entitySchema = schema.GetEntitySchema<DeepParent>();

			var expectedProjections = new(string FieldName, string Alias, string Source, string[] Path)[]
			{
				("Id", "Child_Id", "__joinAlias_Child", new[]{ "Child", "Id" }),
				("Child_Id", "Child_Child_Id", "__joinAlias_Child", new[]{ "Child", "Child", "Id" }),
				("Child_Data", "Child_Child_Data", "__joinAlias_Child", new[]{ "Child", "Child", "Data" })
			};
			CheckForProjections(entitySchema, expectedProjections);

			//  all columns are in table
			var expectedColumns = new(string FieldName, SqlBaseType SqlBaseType)[]
			{
				("FK_Child_Id", SqlBaseType.Int)	//  Child.Child property null check
			};
			CheckForColumns(entitySchema, expectedColumns);
		}

		private static void CheckForProjections(EntitySchema entitySchema,
			(string FieldName, string Alias, string Source, string[] Path)[] expectedProjections)
		{
			foreach (var expectedProjection in expectedProjections)
			{
				var projection = entitySchema.ProjectionFields.FirstOrDefault(q => !q.IsNullCheck && q.FieldName == expectedProjection.FieldName);
				Assert.IsNotNull(projection, $"Projection \"{expectedProjection.FieldName}\" not found.");
				Assert.AreEqual(expectedProjection.Alias, projection.AliasName);
				Assert.AreEqual(expectedProjection.Source, projection.SourceName);
				Assert.IsTrue(expectedProjection.Path.SequenceEqual(projection.ModelPath));
			}
		}

		private static void CheckForColumns(EntitySchema entitySchema,
			(string FieldName, SqlBaseType SqlBaseType)[] expectedColumns)
		{
			foreach (var expectedColumn in expectedColumns)
			{
				var column = entitySchema.EntityTable.Columns.FirstOrDefault(q => q.ColumnName == expectedColumn.FieldName);
				Assert.IsNotNull(column, $"Column \"{expectedColumn.FieldName}\" not found.");
				Assert.AreEqual(expectedColumn.SqlBaseType, column.DataType.BaseType);
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

		private class ConventionPrimaryKey
		{
			public int Id { get; private set; }
		}

		private class CustomPrimaryKey
		{
			public int Id { get; set; }
			public int PrimaryKey { get; private set; }
		}

		private class Parent
		{
			public int Id { get; private set; }
			public Child Child { get; set; }
		}

		private class Child
		{
			public int Id { get; private set; }
			public string Data { get; set; }
		}

		private class DeepParent
		{
			public Parent Child { get; set; }
		}
	}
}
