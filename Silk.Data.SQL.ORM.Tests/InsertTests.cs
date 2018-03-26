using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Operations;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.SQLite3;
using System;

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
			Assert.IsTrue(insert.CanBeBatched);
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
				Assert.IsTrue(insert.CanBeBatched);
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
			Assert.IsTrue(insert.CanBeBatched);
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
				Assert.IsTrue(insert.CanBeBatched);
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

		[TestMethod]
		public void GenerateGuidPK()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PocoWithGuidPK>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<PocoWithGuidPK>();
			var data = new PocoWithGuidPK { Data = "Hello" };
			Assert.AreEqual(Guid.Empty, data.Id);

			var insert = InsertOperation.Create<PocoWithGuidPK>(model, data);
			Assert.IsTrue(insert.CanBeBatched);
			var query = insert.GetQuery();

			using (var provider = new SQLite3DataProvider(":memory:"))
			{
				provider.ExecuteNonQuery(QueryExpression.CreateTable(
					"PocoWithGuidPK",
					QueryExpression.DefineColumn(nameof(PocoWithGuidPK.Id), SqlDataType.Guid()),
					QueryExpression.DefineColumn(nameof(PocoWithGuidPK.Data), SqlDataType.Text())
					));

				provider.ExecuteNonQuery(query);

				Assert.AreNotEqual(Guid.Empty, data.Id);

				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, QueryExpression.Table("PocoWithGuidPK"))))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());
					Assert.AreEqual(data.Id, queryResult.GetGuid(0));
					Assert.AreEqual(data.Data, queryResult.GetString(1));
				}
			}
		}

		[TestMethod]
		public void UseProvidedGuidPK()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PocoWithGuidPK>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<PocoWithGuidPK>();
			var data = new PocoWithGuidPK(Guid.NewGuid()) { Data = "Hello" };
			var specifiedId = data.Id;

			var insert = InsertOperation.Create<PocoWithGuidPK>(model, data);
			Assert.IsTrue(insert.CanBeBatched);
			var query = insert.GetQuery();

			using (var provider = new SQLite3DataProvider(":memory:"))
			{
				provider.ExecuteNonQuery(QueryExpression.CreateTable(
					"PocoWithGuidPK",
					QueryExpression.DefineColumn(nameof(PocoWithGuidPK.Id), SqlDataType.Guid()),
					QueryExpression.DefineColumn(nameof(PocoWithGuidPK.Data), SqlDataType.Text())
					));

				provider.ExecuteNonQuery(query);

				Assert.AreEqual(specifiedId, data.Id);

				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, QueryExpression.Table("PocoWithGuidPK"))))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());
					Assert.AreEqual(specifiedId, queryResult.GetGuid(0));
					Assert.AreEqual(data.Data, queryResult.GetString(1));
				}
			}
		}

		[TestMethod]
		public void GenerateGuidPKWhenAbsentFromView()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PocoWithGuidPK>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<PocoWithGuidPK>();
			var data = new PocoWithGuidPKView { Data = "Hello" };

			var insert = InsertOperation.Create<PocoWithGuidPK, PocoWithGuidPKView>(model, data);
			Assert.IsTrue(insert.CanBeBatched);
			var query = insert.GetQuery();

			using (var provider = new SQLite3DataProvider(":memory:"))
			{
				provider.ExecuteNonQuery(QueryExpression.CreateTable(
					"PocoWithGuidPK",
					QueryExpression.DefineColumn(nameof(PocoWithGuidPK.Id), SqlDataType.Guid()),
					QueryExpression.DefineColumn(nameof(PocoWithGuidPK.Data), SqlDataType.Text())
					));

				provider.ExecuteNonQuery(query);

				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, QueryExpression.Table("PocoWithGuidPK"))))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());
					Assert.AreNotEqual(Guid.Empty, queryResult.GetGuid(0));
					Assert.AreEqual(data.Data, queryResult.GetString(1));
				}
			}
		}

		[TestMethod]
		public void GenerateIntPK()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PocoWithIntPK>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<PocoWithIntPK>();
			var data = new PocoWithIntPK[] {
				new PocoWithIntPK { Data = "Hello" },
				new PocoWithIntPK { Data = "World" }
			};
			Assert.AreEqual(0, data[0].Id);
			Assert.AreEqual(0, data[1].Id);

			var insert = InsertOperation.Create<PocoWithIntPK>(model, data);
			Assert.IsFalse(insert.CanBeBatched);
			var query = insert.GetQuery();
			var builtQuery = new TestQueryConverter().ConvertToQuery(query);

			using (var provider = new SQLite3DataProvider(":memory:"))
			{
				provider.ExecuteNonQuery(QueryExpression.CreateTable(
					"PocoWithIntPK",
					QueryExpression.DefineColumn(nameof(PocoWithIntPK.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(PocoWithIntPK.Data), SqlDataType.Text())
					));

				using (var queryReuslt = provider.ExecuteReader(query))
				{
					insert.ProcessResult(queryReuslt);
				}

				Assert.AreNotEqual(0, data[0].Id);
				Assert.AreNotEqual(0, data[1].Id);
				Assert.AreNotEqual(data[0].Id, data[1].Id);

				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, QueryExpression.Table("PocoWithIntPK"))))
				{
					Assert.IsTrue(queryResult.HasRows);
					foreach (var obj in data)
					{
						Assert.IsTrue(queryResult.Read());
						Assert.AreEqual(obj.Id, queryResult.GetInt32(0));
						Assert.AreEqual(obj.Data, queryResult.GetString(1));
					}
				}
			}
		}

		[TestMethod]
		public void UseProvidedIntPK()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PocoWithIntPK>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<PocoWithIntPK>();
			var data = new PocoWithIntPK[] {
				new PocoWithIntPK(5) { Data = "Hello" }
			};
			var specifiedId = data[0].Id;
			Assert.AreNotEqual(0, data[0].Id);

			var insert = InsertOperation.Create<PocoWithIntPK>(model, data);
			Assert.IsFalse(insert.CanBeBatched);
			var query = insert.GetQuery();
			var builtQuery = new TestQueryConverter().ConvertToQuery(query);

			using (var provider = new SQLite3DataProvider(":memory:"))
			{
				provider.ExecuteNonQuery(QueryExpression.CreateTable(
					"PocoWithIntPK",
					QueryExpression.DefineColumn(nameof(PocoWithIntPK.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(PocoWithIntPK.Data), SqlDataType.Text())
					));

				using (var queryReuslt = provider.ExecuteReader(query))
				{
					insert.ProcessResult(queryReuslt);
				}

				Assert.AreEqual(specifiedId, data[0].Id);

				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, QueryExpression.Table("PocoWithIntPK"))))
				{
					Assert.IsTrue(queryResult.HasRows);
					foreach (var obj in data)
					{
						Assert.IsTrue(queryResult.Read());
						Assert.AreEqual(obj.Id, queryResult.GetInt32(0));
						Assert.AreEqual(obj.Data, queryResult.GetString(1));
					}
				}
			}
		}

		[TestMethod]
		public void GenerateIntPKWhenAbsentFromView()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PocoWithIntPK>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<PocoWithIntPK>();
			var data = new PocoWithIntPKView[] {
				new PocoWithIntPKView { Data = "Hello" },
				new PocoWithIntPKView { Data = "World" }
			};

			var insert = InsertOperation.Create<PocoWithIntPK, PocoWithIntPKView>(model, data);
			Assert.IsFalse(insert.CanBeBatched);
			var query = insert.GetQuery();
			var builtQuery = new TestQueryConverter().ConvertToQuery(query);

			using (var provider = new SQLite3DataProvider(":memory:"))
			{
				provider.ExecuteNonQuery(QueryExpression.CreateTable(
					"PocoWithIntPK",
					QueryExpression.DefineColumn(nameof(PocoWithIntPK.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(PocoWithIntPK.Data), SqlDataType.Text())
					));

				using (var queryReuslt = provider.ExecuteReader(query))
				{
					insert.ProcessResult(queryReuslt);
				}

				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, QueryExpression.Table("PocoWithIntPK"))))
				{
					Assert.IsTrue(queryResult.HasRows);
					var i = 0;
					foreach (var obj in data)
					{
						Assert.IsTrue(queryResult.Read());
						Assert.AreEqual(++i, queryResult.GetInt32(0));
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

		private class PocoWithGuidPK
		{
			public Guid Id { get; private set; }
			public string Data { get; set; }

			public PocoWithGuidPK() { }

			public PocoWithGuidPK(Guid id) { Id = id; }
		}

		private class PocoWithGuidPKView
		{
			public string Data { get; set; }
		}

		private class PocoWithIntPK
		{
			public int Id { get; private set; }
			public string Data { get; set; }

			public PocoWithIntPK() { }

			public PocoWithIntPK(int id) { Id = id; }
		}

		private class PocoWithIntPKView
		{
			public string Data { get; set; }
		}
	}
}
