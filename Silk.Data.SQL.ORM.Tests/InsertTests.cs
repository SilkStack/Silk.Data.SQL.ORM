using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Operations;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class InsertTests
	{
		[TestMethod]
		public void GenerateSimpleInsertSQL()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<SimplePoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<SimplePoco>();

			var data = new SimplePoco[]
			{
				new SimplePoco { Data = "Hello" },
				new SimplePoco { Data = "World" }
			};

			var insert = InsertOperation.Create<SimplePoco>(model, data);
			var insertExpression = insert.GetQuery() as InsertExpression;
			Assert.IsNotNull(insertExpression);
			var query = new TestQueryConverter().ConvertToQuery(insertExpression);
			var sql = TestQueryConverter.CleanSql(query.SqlText);
			Assert.AreEqual(@"INSERT INTO [SimplePoco] ([Data]) VALUES ( @valueParameter1 ) , ( @valueParameter2 ) ;", sql);
			Assert.AreEqual(2, query.QueryParameters.Count);
			Assert.IsTrue(query.QueryParameters.ContainsKey("valueParameter1"));
			Assert.IsTrue(query.QueryParameters.ContainsKey("valueParameter2"));
			Assert.AreEqual(data[0].Data, query.QueryParameters["valueParameter1"].Value);
			Assert.AreEqual(data[1].Data, query.QueryParameters["valueParameter2"].Value);
		}

		private class SimplePoco
		{
			public string Data { get; set; }
		}
	}
}
