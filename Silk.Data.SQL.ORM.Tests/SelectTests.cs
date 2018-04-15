using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Operations;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.SQLite3;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class SelectTests
	{
		[TestMethod]
		public void SelectBulk()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<FlatPoco>();
			var schema = schemaBuilder.Build();
			var entityOperations = new EntityOperations<FlatPoco>(schema);

			using (var provider = new SQLite3DataProvider(":memory:"))
			{
				provider.ExecuteNonReader(CreateTableOperation.Create(schema.GetEntityModel<FlatPoco>().EntityTable));

				var operation = new OperationBuilder()
					.Add(entityOperations.CreateSelect())
					.Add(entityOperations.CreateInsert(new FlatPoco { Data = "Hello" }))
					.Add(entityOperations.CreateSelect())
					.Build();
				var (result1, result2) = provider.ExecuteBulk(operation);
				Assert.IsNotNull(result1);
				Assert.IsNotNull(result2);
				Assert.AreEqual(0, result1.Count);
				Assert.AreEqual(1, result2.Count);
			}
		}

		[TestMethod]
		public void GenerateSelectFlatPocoSQL()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<FlatPoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<FlatPoco>();

			var select = SelectOperation.Create<FlatPoco>(model);
			var sql = TestQueryConverter.CleanSql(
				new TestQueryConverter().ConvertToQuery(select.GetQuery()).SqlText
				);
			Assert.AreEqual(@"SELECT [FlatPoco].[Id] AS [Id], [FlatPoco].[Data] AS [Data]
FROM [FlatPoco];", sql);
		}

		[TestMethod]
		public void QuerySelectFlatPoco()
		{
			using (var sqlProvider = new SQLite3DataProvider(":memory:"))
			{
				sqlProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"FlatPoco",
					QueryExpression.DefineColumn(nameof(FlatPoco.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(FlatPoco.Data), SqlDataType.Text())
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"FlatPoco", new[] { nameof(FlatPoco.Data) },
					new[] { "Hello" },
					new[] { "World" }
					));

				var schemaBuilder = new SchemaBuilder();
				schemaBuilder.DefineEntity<FlatPoco>();
				var schema = schemaBuilder.Build();
				var model = schema.GetEntityModel<FlatPoco>();

				var select = SelectOperation.Create<FlatPoco>(model);
				using (var queryResult = sqlProvider.ExecuteReader(select.GetQuery()))
				{
					select.ProcessResult(queryResult);
				}
				var result = select.Result;

				Assert.IsNotNull(result);
				Assert.AreEqual(2, result.Count);
				Assert.IsTrue(result.Any(q => q.Id == 1 && q.Data == "Hello"));
				Assert.IsTrue(result.Any(q => q.Id == 2 && q.Data == "World"));
			}
		}

		[TestMethod]
		public void GenerateSelectPocoWithSingleRelationshipSQL()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PocoWithSingleRelationship>();
			schemaBuilder.DefineEntity<FlatPoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<PocoWithSingleRelationship>();

			var select = SelectOperation.Create<PocoWithSingleRelationship>(model);
			var sql = TestQueryConverter.CleanSql(
				new TestQueryConverter().ConvertToQuery(select.GetQuery()).SqlText
				);
			Assert.AreEqual(@"SELECT [PocoWithSingleRelationship].[Data] AS [Data], [PocoWithSingleRelationship].[Id] AS [Id], [Data].[Id] AS [Data_Id], [Data].[Data] AS [Data_Data]
FROM [PocoWithSingleRelationship]
LEFT OUTER JOIN [FlatPoco] AS [Data] ON [PocoWithSingleRelationship].[Data] = [Data].[Id];", sql);
		}

		[TestMethod]
		public void QuerySelectPocoWithSingleRelationship()
		{
			using (var sqlProvider = new SQLite3DataProvider(":memory:"))
			{
				sqlProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"FlatPoco",
					QueryExpression.DefineColumn(nameof(FlatPoco.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(FlatPoco.Data), SqlDataType.Text())
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"PocoWithSingleRelationship",
					QueryExpression.DefineColumn(nameof(PocoWithSingleRelationship.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(PocoWithSingleRelationship.Data), SqlDataType.Int(), isNullable: true)
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"FlatPoco", new[] { nameof(FlatPoco.Data) },
					new[] { "Hello" },
					new[] { "World" }
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"PocoWithSingleRelationship", new[] { nameof(PocoWithSingleRelationship.Data) },
					new[] { (object)1 },
					new[] { (object)2 },
					new[] { default(object) }
					));

				var schemaBuilder = new SchemaBuilder();
				schemaBuilder.DefineEntity<FlatPoco>();
				schemaBuilder.DefineEntity<PocoWithSingleRelationship>();
				var schema = schemaBuilder.Build();
				var model = schema.GetEntityModel<PocoWithSingleRelationship>();

				var select = SelectOperation.Create<PocoWithSingleRelationship>(model);
				using (var queryResult = sqlProvider.ExecuteReader(select.GetQuery()))
				{
					select.ProcessResult(queryResult);
				}
				var result = select.Result;

				Assert.IsNotNull(result);
				Assert.AreEqual(3, result.Count);
				Assert.IsTrue(result.Any(q => q.Id == 1 && q.Data.Data == "Hello"));
				Assert.IsTrue(result.Any(q => q.Id == 2 && q.Data.Data == "World"));
				Assert.IsTrue(result.Any(q => q.Id == 3 && q.Data == null));
			}
		}

		[TestMethod]
		public void GenerateSelectPocoWithEmbeddedTypesSQL()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<ClassWithEmbeddedPoco>();
			schemaBuilder.DefineEntity<FlatPoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<ClassWithEmbeddedPoco>();

			var select = SelectOperation.Create<ClassWithEmbeddedPoco>(model);
			var sql = TestQueryConverter.CleanSql(
				new TestQueryConverter().ConvertToQuery(select.GetQuery()).SqlText
				);
			Assert.AreEqual(@"SELECT [ClassWithEmbeddedPoco].[Embedded_SubEmbedded_Relationship] AS [Embedded_SubEmbedded_Relationship], [ClassWithEmbeddedPoco].[Embedded] AS [Embedded], [ClassWithEmbeddedPoco].[Embedded_SubEmbedded] AS [Embedded_SubEmbedded], [ClassWithEmbeddedPoco].[Embedded_SubEmbedded_Data] AS [Embedded_SubEmbedded_Data], [Embedded_SubEmbedded_Relationship].[Id] AS [Embedded_SubEmbedded_Relationship_Id], [Embedded_SubEmbedded_Relationship].[Data] AS [Embedded_SubEmbedded_Relationship_Data], [ClassWithEmbeddedPoco].[Embedded_Data] AS [Embedded_Data], [ClassWithEmbeddedPoco].[Data] AS [Data]
FROM [ClassWithEmbeddedPoco]
LEFT OUTER JOIN [FlatPoco] AS [Embedded_SubEmbedded_Relationship] ON [ClassWithEmbeddedPoco].[Embedded_SubEmbedded_Relationship] = [Embedded_SubEmbedded_Relationship].[Id];", sql);
		}

		[TestMethod]
		public void QuerySelectPocoWithEmbeddedTypes()
		{
			using (var sqlProvider = new SQLite3DataProvider(":memory:"))
			{
				sqlProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"FlatPoco",
					QueryExpression.DefineColumn(nameof(FlatPoco.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(FlatPoco.Data), SqlDataType.Text())
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"ClassWithEmbeddedPoco",
					QueryExpression.DefineColumn("Embedded", SqlDataType.Bit()),
					QueryExpression.DefineColumn("Data", SqlDataType.Text(), isNullable: true),
					QueryExpression.DefineColumn("Embedded_SubEmbedded", SqlDataType.Bit()),
					QueryExpression.DefineColumn("Embedded_Data", SqlDataType.Text(), isNullable: true),
					QueryExpression.DefineColumn("Embedded_SubEmbedded_Relationship", SqlDataType.Int(), isNullable: true),
					QueryExpression.DefineColumn("Embedded_SubEmbedded_Data", SqlDataType.Text(), isNullable: true)
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"FlatPoco", new[] { nameof(FlatPoco.Data) },
					new[] { "Hello" }
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"ClassWithEmbeddedPoco",
					new[] { "Embedded", "Data", "Embedded_SubEmbedded", "Embedded_Data", "Embedded_SubEmbedded_Relationship", "Embedded_SubEmbedded_Data"},
					new[] { (object)1, "Hello World", (object)1, "Hello Embedded World", (object)1, "Hello SubEmbedded World" },
					new[] { (object)1, "Hello World", (object)1, "Hello Embedded World", default(object), "Hello SubEmbedded World" },
					new[] { (object)1, "Hello World", (object)0, "Hello Embedded World", default(object), default(object) }
					));

				var schemaBuilder = new SchemaBuilder();
				schemaBuilder.DefineEntity<FlatPoco>();
				schemaBuilder.DefineEntity<ClassWithEmbeddedPoco>();
				var schema = schemaBuilder.Build();
				var model = schema.GetEntityModel<ClassWithEmbeddedPoco>();

				var select = SelectOperation.Create<ClassWithEmbeddedPoco>(model);
				using (var queryResult = sqlProvider.ExecuteReader(select.GetQuery()))
				{
					select.ProcessResult(queryResult);
				}
				var result = select.Result.ToArray();

				Assert.IsNotNull(result);
				Assert.AreEqual(3, result.Length);

				var instance = result[0];
				Assert.IsNotNull(instance);
				Assert.AreEqual("Hello World", instance.Data);
				Assert.IsNotNull(instance.Embedded);
				Assert.AreEqual("Hello Embedded World", instance.Embedded.Data);
				Assert.IsNotNull(instance.Embedded.SubEmbedded);
				Assert.AreEqual("Hello SubEmbedded World", instance.Embedded.SubEmbedded.Data);
				Assert.IsNotNull(instance.Embedded.SubEmbedded.Relationship);
				Assert.AreEqual("Hello", instance.Embedded.SubEmbedded.Relationship.Data);

				instance = result[1];
				Assert.IsNotNull(instance);
				Assert.AreEqual("Hello World", instance.Data);
				Assert.IsNotNull(instance.Embedded);
				Assert.AreEqual("Hello Embedded World", instance.Embedded.Data);
				Assert.IsNotNull(instance.Embedded.SubEmbedded);
				Assert.AreEqual("Hello SubEmbedded World", instance.Embedded.SubEmbedded.Data);
				Assert.IsNull(instance.Embedded.SubEmbedded.Relationship);

				instance = result[2];
				Assert.IsNotNull(instance);
				Assert.AreEqual("Hello World", instance.Data);
				Assert.IsNotNull(instance.Embedded);
				Assert.AreEqual("Hello Embedded World", instance.Embedded.Data);
				Assert.IsNull(instance.Embedded.SubEmbedded);
			}
		}

		[TestMethod]
		public void GenerateSelectPocoWithMultileRelatedObjectsSQL()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PocoWithManyRelationship>();
			schemaBuilder.DefineEntity<FlatPoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<PocoWithManyRelationship>();

			var select = SelectOperation.Create<PocoWithManyRelationship>(model);
			var sql = TestQueryConverter.CleanSql(
				new TestQueryConverter().ConvertToQuery(select.GetQuery()).SqlText
				);
			Assert.AreEqual(@"SELECT [PocoWithManyRelationship].[Id] AS [Id]
FROM [PocoWithManyRelationship];
SELECT [PocoWithManyRelationship].[Id] AS [__IDENT__Data], [Data].[Id] AS [Id], [Data].[Data] AS [Data]
FROM [PocoWithManyRelationship]
INNER JOIN [PocoWithManyRelationship_DataToFlatPoco] AS [__JUNCTION__Data] ON [PocoWithManyRelationship].[Id] = [__JUNCTION__Data].[LocalKey]
INNER JOIN [FlatPoco] AS [Data] ON [__JUNCTION__Data].[RemoteKey] = [Data].[Id];", sql);
		}

		[TestMethod]
		public void QuerySelectPocoWithMultileRelatedObjects()
		{
			using (var sqlProvider = new SQLite3DataProvider(":memory:"))
			{
				sqlProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"FlatPoco",
					QueryExpression.DefineColumn(nameof(FlatPoco.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(FlatPoco.Data), SqlDataType.Text())
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"PocoWithManyRelationship",
					QueryExpression.DefineColumn(nameof(PocoWithManyRelationship.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true)
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"PocoWithManyRelationship_DataToFlatPoco",
					QueryExpression.DefineColumn("LocalKey", SqlDataType.Int()),
					QueryExpression.DefineColumn("RemoteKey", SqlDataType.Int())
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"FlatPoco", new[] { nameof(FlatPoco.Data) },
					new[] { "Hello" },
					new[] { "World" }
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"PocoWithManyRelationship", new[] { nameof(PocoWithManyRelationship.Id) },
					new[] { default(object) },
					new[] { default(object) },
					new[] { default(object) }
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"PocoWithManyRelationship_DataToFlatPoco",
					new[] { "LocalKey", "RemoteKey" },
					new[] { (object)1, (object)1 },
					new[] { (object)1, (object)2 },
					new[] { (object)2, (object)1 }
					));

				var schemaBuilder = new SchemaBuilder();
				schemaBuilder.DefineEntity<FlatPoco>();
				schemaBuilder.DefineEntity<PocoWithManyRelationship>();
				var schema = schemaBuilder.Build();
				var model = schema.GetEntityModel<PocoWithManyRelationship>();

				var select = SelectOperation.Create<PocoWithManyRelationship>(model);
				using (var queryResult = sqlProvider.ExecuteReader(select.GetQuery()))
				{
					select.ProcessResult(queryResult);
				}
				var result = select.Result.ToArray();

				Assert.IsNotNull(result);
				Assert.AreEqual(3, result.Length);

				var instance = result.FirstOrDefault(q => q.Id == 1);
				Assert.IsNotNull(instance);
				Assert.IsNotNull(instance.Data);
				Assert.AreEqual(2, instance.Data.Count);
				var subInstance = instance.Data.FirstOrDefault(q => q.Id == 1);
				Assert.IsNotNull(subInstance);
				Assert.AreEqual("Hello", subInstance.Data);
				subInstance = instance.Data.FirstOrDefault(q => q.Id == 2);
				Assert.IsNotNull(subInstance);
				Assert.AreEqual("World", subInstance.Data);

				instance = result.FirstOrDefault(q => q.Id == 2);
				Assert.IsNotNull(instance);
				Assert.IsNotNull(instance.Data);
				Assert.AreEqual(1, instance.Data.Count);
				subInstance = instance.Data.FirstOrDefault(q => q.Id == 1);
				Assert.IsNotNull(subInstance);
				Assert.AreEqual("Hello", subInstance.Data);

				instance = result.FirstOrDefault(q => q.Id == 3);
				Assert.IsNotNull(instance);
				Assert.IsNotNull(instance.Data);
				Assert.AreEqual(0, instance.Data.Count);
			}
		}

		[TestMethod]
		public void ProjectionGenerateSelectFlatPocoSQL()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<FlatPoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<FlatPoco>();

			var select = SelectOperation.Create<FlatPoco, FlatPocoProjection>(model);
			var sql = TestQueryConverter.CleanSql(
				new TestQueryConverter().ConvertToQuery(select.GetQuery()).SqlText
				);
			Assert.AreEqual(@"SELECT [FlatPoco].[Data] AS [Data]
FROM [FlatPoco];", sql);
		}

		[TestMethod]
		public void ProjectionQuerySelectFlatPoco()
		{
			using (var sqlProvider = new SQLite3DataProvider(":memory:"))
			{
				sqlProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"FlatPoco",
					QueryExpression.DefineColumn(nameof(FlatPoco.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(FlatPoco.Data), SqlDataType.Text())
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"FlatPoco", new[] { nameof(FlatPoco.Data) },
					new[] { "Hello" },
					new[] { "World" }
					));

				var schemaBuilder = new SchemaBuilder();
				schemaBuilder.DefineEntity<FlatPoco>();
				var schema = schemaBuilder.Build();
				var model = schema.GetEntityModel<FlatPoco>();

				var select = SelectOperation.Create<FlatPoco, FlatPocoProjection>(model);
				using (var queryResult = sqlProvider.ExecuteReader(select.GetQuery()))
				{
					select.ProcessResult(queryResult);
				}
				var result = select.Result;

				Assert.IsNotNull(result);
				Assert.AreEqual(2, result.Count);
				Assert.IsTrue(result.Any(q => q.Data == "Hello"));
				Assert.IsTrue(result.Any(q => q.Data == "World"));
			}
		}

		[TestMethod]
		public void FlattenedProjectionGenerateSelectPocoWithSingleRelationshipSQL()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PocoWithSingleRelationship>();
			schemaBuilder.DefineEntity<FlatPoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<PocoWithSingleRelationship>();

			var select = SelectOperation.Create<PocoWithSingleRelationship, ProjectionPocoWithSingleRelationship>(model);
			var sql = TestQueryConverter.CleanSql(
				new TestQueryConverter().ConvertToQuery(select.GetQuery()).SqlText
				);
			Assert.AreEqual(@"SELECT [PocoWithSingleRelationship].[Data] AS [Data], [PocoWithSingleRelationship].[Id] AS [Id], [Data].[Data] AS [Data_Data]
FROM [PocoWithSingleRelationship]
LEFT OUTER JOIN [FlatPoco] AS [Data] ON [PocoWithSingleRelationship].[Data] = [Data].[Id];", sql);
		}

		[TestMethod]
		public void FlattenedProjectionQuerySelectPocoWithSingleRelationship()
		{
			using (var sqlProvider = new SQLite3DataProvider(":memory:"))
			{
				sqlProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"FlatPoco",
					QueryExpression.DefineColumn(nameof(FlatPoco.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(FlatPoco.Data), SqlDataType.Text())
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"PocoWithSingleRelationship",
					QueryExpression.DefineColumn(nameof(PocoWithSingleRelationship.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(PocoWithSingleRelationship.Data), SqlDataType.Int(), isNullable: true)
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"FlatPoco", new[] { nameof(FlatPoco.Data) },
					new[] { "Hello" },
					new[] { "World" }
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"PocoWithSingleRelationship", new[] { nameof(PocoWithSingleRelationship.Data) },
					new[] { (object)1 },
					new[] { (object)2 },
					new[] { default(object) }
					));

				var schemaBuilder = new SchemaBuilder();
				schemaBuilder.DefineEntity<FlatPoco>();
				schemaBuilder.DefineEntity<PocoWithSingleRelationship>();
				var schema = schemaBuilder.Build();
				var model = schema.GetEntityModel<PocoWithSingleRelationship>();

				var select = SelectOperation.Create<PocoWithSingleRelationship, ProjectionPocoWithSingleRelationship>(model);
				using (var queryResult = sqlProvider.ExecuteReader(select.GetQuery()))
				{
					select.ProcessResult(queryResult);
				}
				var result = select.Result;

				Assert.IsNotNull(result);
				Assert.AreEqual(3, result.Count);
				Assert.IsTrue(result.Any(q => q.Id == 1 && q.DataData == "Hello"));
				Assert.IsTrue(result.Any(q => q.Id == 2 && q.DataData == "World"));
				Assert.IsTrue(result.Any(q => q.Id == 3 && q.DataData == null));
			}
		}

		[TestMethod]
		public void FlattenedProjectionGenerateSelectPocoWithEmbeddedTypesSQL()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<ClassWithEmbeddedPoco>();
			schemaBuilder.DefineEntity<FlatPoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<ClassWithEmbeddedPoco>();

			var select = SelectOperation.Create<ClassWithEmbeddedPoco, FlatProjectionClassWithEmbeddedPoco>(model);
			var sql = TestQueryConverter.CleanSql(
				new TestQueryConverter().ConvertToQuery(select.GetQuery()).SqlText
				);
			Assert.AreEqual(@"SELECT [ClassWithEmbeddedPoco].[Embedded_SubEmbedded_Relationship] AS [Embedded_SubEmbedded_Relationship], [ClassWithEmbeddedPoco].[Embedded] AS [Embedded], [ClassWithEmbeddedPoco].[Embedded_SubEmbedded] AS [Embedded_SubEmbedded], [ClassWithEmbeddedPoco].[Embedded_SubEmbedded_Data] AS [Embedded_SubEmbedded_Data], [Embedded_SubEmbedded_Relationship].[Data] AS [Embedded_SubEmbedded_Relationship_Data], [ClassWithEmbeddedPoco].[Embedded_Data] AS [Embedded_Data], [ClassWithEmbeddedPoco].[Data] AS [Data]
FROM [ClassWithEmbeddedPoco]
LEFT OUTER JOIN [FlatPoco] AS [Embedded_SubEmbedded_Relationship] ON [ClassWithEmbeddedPoco].[Embedded_SubEmbedded_Relationship] = [Embedded_SubEmbedded_Relationship].[Id];", sql);
		}

		[TestMethod]
		public void FlattenedProjectionQuerySelectPocoWithEmbeddedTypes()
		{
			using (var sqlProvider = new SQLite3DataProvider(":memory:"))
			{
				sqlProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"FlatPoco",
					QueryExpression.DefineColumn(nameof(FlatPoco.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(FlatPoco.Data), SqlDataType.Text())
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"ClassWithEmbeddedPoco",
					QueryExpression.DefineColumn("Embedded", SqlDataType.Bit()),
					QueryExpression.DefineColumn("Data", SqlDataType.Text(), isNullable: true),
					QueryExpression.DefineColumn("Embedded_SubEmbedded", SqlDataType.Bit()),
					QueryExpression.DefineColumn("Embedded_Data", SqlDataType.Text(), isNullable: true),
					QueryExpression.DefineColumn("Embedded_SubEmbedded_Relationship", SqlDataType.Int(), isNullable: true),
					QueryExpression.DefineColumn("Embedded_SubEmbedded_Data", SqlDataType.Text(), isNullable: true)
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"FlatPoco", new[] { nameof(FlatPoco.Data) },
					new[] { "Hello" }
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"ClassWithEmbeddedPoco",
					new[] { "Embedded", "Data", "Embedded_SubEmbedded", "Embedded_Data", "Embedded_SubEmbedded_Relationship", "Embedded_SubEmbedded_Data" },
					new[] { (object)1, "Hello World", (object)1, "Hello Embedded World", (object)1, "Hello SubEmbedded World" },
					new[] { (object)1, "Hello World", (object)1, "Hello Embedded World", default(object), "Hello SubEmbedded World" },
					new[] { (object)1, "Hello World", (object)0, "Hello Embedded World", default(object), default(object) }
					));

				var schemaBuilder = new SchemaBuilder();
				schemaBuilder.DefineEntity<FlatPoco>();
				schemaBuilder.DefineEntity<ClassWithEmbeddedPoco>();
				var schema = schemaBuilder.Build();
				var model = schema.GetEntityModel<ClassWithEmbeddedPoco>();

				var select = SelectOperation.Create<ClassWithEmbeddedPoco, FlatProjectionClassWithEmbeddedPoco>(model);
				using (var queryResult = sqlProvider.ExecuteReader(select.GetQuery()))
				{
					select.ProcessResult(queryResult);
				}
				var result = select.Result.ToArray();

				Assert.IsNotNull(result);
				Assert.AreEqual(3, result.Length);

				var instance = result[0];
				Assert.IsNotNull(instance);
				Assert.AreEqual("Hello World", instance.Data);
				Assert.IsNotNull(instance.EmbeddedData);
				Assert.AreEqual("Hello Embedded World", instance.EmbeddedData);
				Assert.IsNotNull(instance.EmbeddedSubEmbeddedData);
				Assert.AreEqual("Hello SubEmbedded World", instance.EmbeddedSubEmbeddedData);
				Assert.IsNotNull(instance.EmbeddedSubEmbeddedRelationshipData);
				Assert.AreEqual("Hello", instance.EmbeddedSubEmbeddedRelationshipData);

				instance = result[1];
				Assert.IsNotNull(instance);
				Assert.AreEqual("Hello World", instance.Data);
				Assert.IsNotNull(instance.EmbeddedData);
				Assert.AreEqual("Hello Embedded World", instance.EmbeddedData);
				Assert.IsNotNull(instance.EmbeddedSubEmbeddedData);
				Assert.AreEqual("Hello SubEmbedded World", instance.EmbeddedSubEmbeddedData);
				Assert.IsNull(instance.EmbeddedSubEmbeddedRelationshipData);

				instance = result[2];
				Assert.IsNotNull(instance);
				Assert.AreEqual("Hello World", instance.Data);
				Assert.IsNotNull(instance.EmbeddedData);
				Assert.AreEqual("Hello Embedded World", instance.EmbeddedData);
				Assert.IsNull(instance.EmbeddedSubEmbeddedData);
				Assert.IsNull(instance.EmbeddedSubEmbeddedRelationshipData);
			}
		}

		[TestMethod]
		public void ProjectIdenticalGenerateSelectPocoWithEmbeddedTypesSQL()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<ClassWithEmbeddedPoco>();
			schemaBuilder.DefineEntity<FlatPoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<ClassWithEmbeddedPoco>();

			var select = SelectOperation.Create<ClassWithEmbeddedPoco, IdenticalProjectionOfClassWithEmbeddedPoco>(model);
			var sql = TestQueryConverter.CleanSql(
				new TestQueryConverter().ConvertToQuery(select.GetQuery()).SqlText
				);
			Assert.AreEqual(@"SELECT [ClassWithEmbeddedPoco].[Embedded_SubEmbedded_Relationship] AS [Embedded_SubEmbedded_Relationship], [ClassWithEmbeddedPoco].[Embedded] AS [Embedded], [ClassWithEmbeddedPoco].[Embedded_SubEmbedded] AS [Embedded_SubEmbedded], [ClassWithEmbeddedPoco].[Embedded_SubEmbedded_Data] AS [Embedded_SubEmbedded_Data], [Embedded_SubEmbedded_Relationship].[Id] AS [Embedded_SubEmbedded_Relationship_Id], [Embedded_SubEmbedded_Relationship].[Data] AS [Embedded_SubEmbedded_Relationship_Data], [ClassWithEmbeddedPoco].[Embedded_Data] AS [Embedded_Data], [ClassWithEmbeddedPoco].[Data] AS [Data]
FROM [ClassWithEmbeddedPoco]
LEFT OUTER JOIN [FlatPoco] AS [Embedded_SubEmbedded_Relationship] ON [ClassWithEmbeddedPoco].[Embedded_SubEmbedded_Relationship] = [Embedded_SubEmbedded_Relationship].[Id];", sql);
		}

		[TestMethod]
		public void ProjectIdenticalQuerySelectPocoWithEmbeddedTypes()
		{
			using (var sqlProvider = new SQLite3DataProvider(":memory:"))
			{
				sqlProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"FlatPoco",
					QueryExpression.DefineColumn(nameof(FlatPoco.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(FlatPoco.Data), SqlDataType.Text())
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"ClassWithEmbeddedPoco",
					QueryExpression.DefineColumn("Embedded", SqlDataType.Bit()),
					QueryExpression.DefineColumn("Data", SqlDataType.Text(), isNullable: true),
					QueryExpression.DefineColumn("Embedded_SubEmbedded", SqlDataType.Bit()),
					QueryExpression.DefineColumn("Embedded_Data", SqlDataType.Text(), isNullable: true),
					QueryExpression.DefineColumn("Embedded_SubEmbedded_Relationship", SqlDataType.Int(), isNullable: true),
					QueryExpression.DefineColumn("Embedded_SubEmbedded_Data", SqlDataType.Text(), isNullable: true)
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"FlatPoco", new[] { nameof(FlatPoco.Data) },
					new[] { "Hello" }
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"ClassWithEmbeddedPoco",
					new[] { "Embedded", "Data", "Embedded_SubEmbedded", "Embedded_Data", "Embedded_SubEmbedded_Relationship", "Embedded_SubEmbedded_Data" },
					new[] { (object)1, "Hello World", (object)1, "Hello Embedded World", (object)1, "Hello SubEmbedded World" },
					new[] { (object)1, "Hello World", (object)1, "Hello Embedded World", default(object), "Hello SubEmbedded World" },
					new[] { (object)1, "Hello World", (object)0, "Hello Embedded World", default(object), default(object) }
					));

				var schemaBuilder = new SchemaBuilder();
				schemaBuilder.DefineEntity<FlatPoco>();
				schemaBuilder.DefineEntity<ClassWithEmbeddedPoco>();
				var schema = schemaBuilder.Build();
				var model = schema.GetEntityModel<ClassWithEmbeddedPoco>();

				var select = SelectOperation.Create<ClassWithEmbeddedPoco, IdenticalProjectionOfClassWithEmbeddedPoco>(model);
				using (var queryResult = sqlProvider.ExecuteReader(select.GetQuery()))
				{
					select.ProcessResult(queryResult);
				}
				var result = select.Result.ToArray();

				Assert.IsNotNull(result);
				Assert.AreEqual(3, result.Length);

				var instance = result[0];
				Assert.IsNotNull(instance);
				Assert.AreEqual("Hello World", instance.Data);
				Assert.IsNotNull(instance.Embedded);
				Assert.AreEqual("Hello Embedded World", instance.Embedded.Data);
				Assert.IsNotNull(instance.Embedded.SubEmbedded);
				Assert.AreEqual("Hello SubEmbedded World", instance.Embedded.SubEmbedded.Data);
				Assert.IsNotNull(instance.Embedded.SubEmbedded.Relationship);
				Assert.AreEqual("Hello", instance.Embedded.SubEmbedded.Relationship.Data);

				instance = result[1];
				Assert.IsNotNull(instance);
				Assert.AreEqual("Hello World", instance.Data);
				Assert.IsNotNull(instance.Embedded);
				Assert.AreEqual("Hello Embedded World", instance.Embedded.Data);
				Assert.IsNotNull(instance.Embedded.SubEmbedded);
				Assert.AreEqual("Hello SubEmbedded World", instance.Embedded.SubEmbedded.Data);
				Assert.IsNull(instance.Embedded.SubEmbedded.Relationship);

				instance = result[2];
				Assert.IsNotNull(instance);
				Assert.AreEqual("Hello World", instance.Data);
				Assert.IsNotNull(instance.Embedded);
				Assert.AreEqual("Hello Embedded World", instance.Embedded.Data);
				Assert.IsNull(instance.Embedded.SubEmbedded);
			}
		}

		[TestMethod]
		public void SubProjectGenerateSelectPocoWithEmbeddedTypesSQL()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<ClassWithEmbeddedPoco>();
			schemaBuilder.DefineEntity<FlatPoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<ClassWithEmbeddedPoco>();

			var select = SelectOperation.Create<ClassWithEmbeddedPoco, ProjectionClassWithEmbeddedPoco>(model);
			var sql = TestQueryConverter.CleanSql(
				new TestQueryConverter().ConvertToQuery(select.GetQuery()).SqlText
				);
			Assert.AreEqual(@"SELECT [ClassWithEmbeddedPoco].[Embedded_SubEmbedded_Relationship] AS [Embedded_SubEmbedded_Relationship], [ClassWithEmbeddedPoco].[Embedded] AS [Embedded], [ClassWithEmbeddedPoco].[Embedded_SubEmbedded] AS [Embedded_SubEmbedded], [ClassWithEmbeddedPoco].[Embedded_SubEmbedded_Data] AS [Embedded_SubEmbedded_Data], [Embedded_SubEmbedded_Relationship].[Id] AS [Embedded_SubEmbedded_Relationship_Id], [Embedded_SubEmbedded_Relationship].[Data] AS [Embedded_SubEmbedded_Relationship_Data]
FROM [ClassWithEmbeddedPoco]
LEFT OUTER JOIN [FlatPoco] AS [Embedded_SubEmbedded_Relationship] ON [ClassWithEmbeddedPoco].[Embedded_SubEmbedded_Relationship] = [Embedded_SubEmbedded_Relationship].[Id];", sql);
		}

		[TestMethod]
		public void SubProjectQuerySelectPocoWithEmbeddedTypes()
		{
			using (var sqlProvider = new SQLite3DataProvider(":memory:"))
			{
				sqlProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"FlatPoco",
					QueryExpression.DefineColumn(nameof(FlatPoco.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(FlatPoco.Data), SqlDataType.Text())
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"ClassWithEmbeddedPoco",
					QueryExpression.DefineColumn("Embedded", SqlDataType.Bit()),
					QueryExpression.DefineColumn("Data", SqlDataType.Text(), isNullable: true),
					QueryExpression.DefineColumn("Embedded_SubEmbedded", SqlDataType.Bit()),
					QueryExpression.DefineColumn("Embedded_Data", SqlDataType.Text(), isNullable: true),
					QueryExpression.DefineColumn("Embedded_SubEmbedded_Relationship", SqlDataType.Int(), isNullable: true),
					QueryExpression.DefineColumn("Embedded_SubEmbedded_Data", SqlDataType.Text(), isNullable: true)
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"FlatPoco", new[] { nameof(FlatPoco.Data) },
					new[] { "Hello" }
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"ClassWithEmbeddedPoco",
					new[] { "Embedded", "Data", "Embedded_SubEmbedded", "Embedded_Data", "Embedded_SubEmbedded_Relationship", "Embedded_SubEmbedded_Data" },
					new[] { (object)1, "Hello World", (object)1, "Hello Embedded World", (object)1, "Hello SubEmbedded World" },
					new[] { (object)1, "Hello World", (object)1, "Hello Embedded World", default(object), "Hello SubEmbedded World" },
					new[] { (object)1, "Hello World", (object)0, "Hello Embedded World", default(object), default(object) }
					));

				var schemaBuilder = new SchemaBuilder();
				schemaBuilder.DefineEntity<FlatPoco>();
				schemaBuilder.DefineEntity<ClassWithEmbeddedPoco>();
				var schema = schemaBuilder.Build();
				var model = schema.GetEntityModel<ClassWithEmbeddedPoco>();

				var select = SelectOperation.Create<ClassWithEmbeddedPoco, ProjectionClassWithEmbeddedPoco>(model);
				using (var queryResult = sqlProvider.ExecuteReader(select.GetQuery()))
				{
					select.ProcessResult(queryResult);
				}
				var result = select.Result.ToArray();

				Assert.IsNotNull(result);
				Assert.AreEqual(3, result.Length);

				var instance = result[0];
				Assert.IsNotNull(instance);
				Assert.AreEqual("Hello SubEmbedded World", instance.Embedded.SubEmbeddedData);
				Assert.IsNotNull(instance.Embedded.SubEmbeddedRelationship);
				Assert.AreEqual("Hello", instance.Embedded.SubEmbeddedRelationship.Data);

				instance = result[1];
				Assert.IsNotNull(instance);
				Assert.AreEqual("Hello SubEmbedded World", instance.Embedded.SubEmbeddedData);
				Assert.IsNull(instance.Embedded.SubEmbeddedRelationship);

				instance = result[2];
				Assert.IsNotNull(instance);
				Assert.IsNull(instance.Embedded.SubEmbeddedData);
				Assert.IsNull(instance.Embedded.SubEmbeddedRelationship);
			}
		}

		[TestMethod]
		public void QuerySelectPocoWithNullable()
		{
			using (var sqlProvider = new SQLite3DataProvider(":memory:"))
			{
				sqlProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"PocoWithNullable",
					QueryExpression.DefineColumn(nameof(PocoWithNullable.NullableInt), SqlDataType.Int(), isNullable: true)
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"PocoWithNullable", new[] { nameof(PocoWithNullable.NullableInt) },
					new[] { (object)1 },
					new[] { default(object) }
					));

				var schemaBuilder = new SchemaBuilder();
				schemaBuilder.DefineEntity<PocoWithNullable>();
				var schema = schemaBuilder.Build();
				var model = schema.GetEntityModel<PocoWithNullable>();

				var select = SelectOperation.Create<PocoWithNullable>(model);
				using (var queryResult = sqlProvider.ExecuteReader(select.GetQuery()))
				{
					select.ProcessResult(queryResult);
				}
				var result = select.Result.ToArray();

				Assert.IsNotNull(result);
				Assert.AreEqual(2, result.Length);

				var instance = result[0];
				Assert.IsNotNull(instance);
				Assert.AreEqual(1, instance.NullableInt);

				instance = result[1];
				Assert.IsNotNull(instance);
				Assert.IsNull(instance.NullableInt);
			}
		}

		private class FlatPoco
		{
			public int Id { get; set; }
			public string Data { get; set; }
		}

		private class FlatPocoProjection
		{
			public string Data { get; set; }
		}

		private class PocoWithSingleRelationship
		{
			public int Id { get; set; }
			public FlatPoco Data { get; set; }
		}

		private class ProjectionPocoWithSingleRelationship
		{
			public int Id { get; set; }
			public string DataData { get; set; }
		}

		private class PocoWithManyRelationship
		{
			public int Id { get; set; }
			public List<FlatPoco> Data { get; set; }
		}

		private class ClassWithEmbeddedPoco
		{
			public EmbeddedPocoTier1 Embedded { get; set; }
			public string Data { get; set; }
		}

		private class IdenticalProjectionOfClassWithEmbeddedPoco
		{
			public EmbeddedPocoTier1 Embedded { get; set; }
			public string Data { get; set; }
		}

		private class ProjectionClassWithEmbeddedPoco
		{
			public ProjectionEmbeddedPocoTier1 Embedded { get; set; }
		}

		private class ProjectionEmbeddedPocoTier1
		{
			public FlatPoco SubEmbeddedRelationship { get; set; }
			public string SubEmbeddedData { get; set; }
		}

		private class FlatProjectionClassWithEmbeddedPoco
		{
			public string EmbeddedData { get; set; }
			public string EmbeddedSubEmbeddedData { get; set; }
			public string EmbeddedSubEmbeddedRelationshipData { get; set; }
			public string Data { get; set; }
		}

		private class EmbeddedPocoTier1
		{
			public EmbeddedPocoTier2 SubEmbedded { get; set; }
			public string Data { get; set; }
		}

		private class EmbeddedPocoTier2
		{
			public FlatPoco Relationship { get; set; }
			public string Data { get; set; }
		}

		private class PocoWithNullable
		{
			public int? NullableInt { get; set; }
		}
	}
}
