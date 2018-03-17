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
			Assert.AreEqual("SELECT [FlatPoco].[Id] AS [Id], [FlatPoco].[Data] AS [Data] FROM [FlatPoco];", sql);
		}

		private class FlatPoco
		{
			public int Id { get; set; }
			public string Data { get; set; }
		}
	}
}
