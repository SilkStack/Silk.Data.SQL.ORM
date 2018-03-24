using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Operations;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.SQLite3;

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
				new SimplePoco { Int = 1, Data = "Hello" },
				new SimplePoco { Int = 2, Data = "World" }
			};

			var insert = InsertOperation.Create<SimplePoco>(model, data);
			var insertExpression = insert.GetQuery() as InsertExpression;
			Assert.IsNotNull(insertExpression);
			var query = new TestQueryConverter().ConvertToQuery(insertExpression);
			var sql = TestQueryConverter.CleanSql(query.SqlText);
			Assert.AreEqual(@"INSERT INTO [SimplePoco] ([Int], [Data]) VALUES ( @valueParameter1 , @valueParameter2 ) , ( @valueParameter3 , @valueParameter4 ) ;", sql);
			Assert.AreEqual(4, query.QueryParameters.Count);
			Assert.IsTrue(query.QueryParameters.ContainsKey("valueParameter1"));
			Assert.IsTrue(query.QueryParameters.ContainsKey("valueParameter2"));
			Assert.IsTrue(query.QueryParameters.ContainsKey("valueParameter3"));
			Assert.IsTrue(query.QueryParameters.ContainsKey("valueParameter4"));
			Assert.AreEqual(data[0].Int, query.QueryParameters["valueParameter1"].Value);
			Assert.AreEqual(data[0].Data, query.QueryParameters["valueParameter2"].Value);
			Assert.AreEqual(data[1].Int, query.QueryParameters["valueParameter3"].Value);
			Assert.AreEqual(data[1].Data, query.QueryParameters["valueParameter4"].Value);
		}

		[TestMethod]
		public void SimpleInsertQuery()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<SimplePoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<SimplePoco>();

			using (var provider = new SQLite3DataProvider(":memory:"))
			{
				provider.ExecuteNonQuery(QueryExpression.CreateTable(
					"SimplePoco",
					QueryExpression.DefineColumn("Int", SqlDataType.Int()),
					QueryExpression.DefineColumn("Data", SqlDataType.Text())
					));

				var data = new SimplePoco[]
				{
					new SimplePoco { Int = 1, Data = "Hello" },
					new SimplePoco { Int = 2, Data = "World" }
				};

				var insert = InsertOperation.Create<SimplePoco>(model, data);
				using (var queryResult = provider.ExecuteReader(insert.GetQuery()))
				{
					insert.ProcessResult(queryResult);
				}

				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, QueryExpression.Table("SimplePoco"))))
				{
					Assert.IsTrue(queryResult.HasRows);
					foreach(var obj in data)
					{
						Assert.IsTrue(queryResult.Read());
						Assert.AreEqual(obj.Int, queryResult.GetInt32(0));
						Assert.AreEqual(obj.Data, queryResult.GetString(1));
					}
				}
			}
		}

		[TestMethod]
		public void GenerateSimpleInsertSQLForView()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<SimplePoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<SimplePoco>();

			var data = new SimplePocoDataView[]
			{
				new SimplePocoDataView { Data = "Hello" },
				new SimplePocoDataView { Data = "World" }
			};

			var insert = InsertOperation.Create<SimplePoco, SimplePocoDataView>(model, data);
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

		[TestMethod]
		public void SimpleInsertQueryForView()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<SimplePoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<SimplePoco>();

			using (var provider = new SQLite3DataProvider(":memory:"))
			{
				provider.ExecuteNonQuery(QueryExpression.CreateTable(
					"SimplePoco",
					QueryExpression.DefineColumn("Int", SqlDataType.Int(), isNullable: true),
					QueryExpression.DefineColumn("Data", SqlDataType.Text())
					));

				var data = new SimplePocoDataView[]
				{
					new SimplePocoDataView { Data = "Hello" },
					new SimplePocoDataView { Data = "World" }
				};

				var insert = InsertOperation.Create<SimplePoco, SimplePocoDataView>(model, data);
				using (var queryResult = provider.ExecuteReader(insert.GetQuery()))
				{
					insert.ProcessResult(queryResult);
				}

				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, QueryExpression.Table("SimplePoco"))))
				{
					Assert.IsTrue(queryResult.HasRows);
					foreach (var obj in data)
					{
						Assert.IsTrue(queryResult.Read());
						Assert.AreEqual(obj.Data, queryResult.GetString(1));
					}
				}
			}
		}

		private class SimplePoco
		{
			public int? Int { get; set; }
			public string Data { get; set; }
		}

		private class SimplePocoDataView
		{
			public string Data { get; set; }
		}
	}
}
