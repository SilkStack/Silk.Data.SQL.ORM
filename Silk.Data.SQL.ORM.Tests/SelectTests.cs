using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Operations;
using Silk.Data.SQL.ORM.Schema;

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
