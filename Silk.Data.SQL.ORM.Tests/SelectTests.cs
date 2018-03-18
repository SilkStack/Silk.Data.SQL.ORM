using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Operations;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.SQLite3;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class SelectTests
	{
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
			Assert.AreEqual(@"SELECT [PocoWithSingleRelationship].[Id] AS [Id], [Data].[Id] AS [Data_Id], [Data].[Data] AS [Data_Data]
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
					QueryExpression.DefineColumn(nameof(PocoWithSingleRelationship.Data), SqlDataType.Int())
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"FlatPoco", new[] { nameof(FlatPoco.Data) },
					new[] { "Hello" },
					new[] { "World" }
					));

				sqlProvider.ExecuteNonQuery(QueryExpression.Insert(
					"PocoWithSingleRelationship", new[] { nameof(PocoWithSingleRelationship.Data) },
					new[] { (object)1 },
					new[] { (object)2 }
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
				Assert.AreEqual(2, result.Count);
				Assert.IsTrue(result.Any(q => q.Id == 1 && q.Data.Data == "Hello"));
				Assert.IsTrue(result.Any(q => q.Id == 2 && q.Data.Data == "World"));
			}
		}

		private class FlatPoco
		{
			public int Id { get; set; }
			public string Data { get; set; }
		}

		private class PocoWithSingleRelationship
		{
			public int Id { get; set; }
			public FlatPoco Data { get; set; }
		}
	}
}
