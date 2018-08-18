using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Operations;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.SQLite3;
using System;
using System.Collections.Generic;
using System.Linq;

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
			var insertExpression = insert.GetQuery();
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

			using (var provider = new SQLite3DataProvider(TestHelper.ConnectionString))
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
			var insertExpression = insert.GetQuery();
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

			using (var provider = new SQLite3DataProvider(TestHelper.ConnectionString))
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

			using (var provider = new SQLite3DataProvider(TestHelper.ConnectionString))
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

			using (var provider = new SQLite3DataProvider(TestHelper.ConnectionString))
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

			using (var provider = new SQLite3DataProvider(TestHelper.ConnectionString))
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

			using (var provider = new SQLite3DataProvider(TestHelper.ConnectionString))
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
			Assert.IsTrue(insert.CanBeBatched);
			var query = insert.GetQuery();
			var builtQuery = new TestQueryConverter().ConvertToQuery(query);

			using (var provider = new SQLite3DataProvider(TestHelper.ConnectionString))
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
		public void MixedProvidedAndGeneratedIntPK()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PocoWithIntPK>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<PocoWithIntPK>();
			var data = new PocoWithIntPK[] {
				new PocoWithIntPK(5) { Data = "Hello" },
				new PocoWithIntPK(6) { Data = "World" },
				new PocoWithIntPK() { Data = "Redux" }
			};
			Assert.AreEqual(5, data[0].Id);
			Assert.AreEqual(6, data[1].Id);
			Assert.AreEqual(0, data[2].Id);

			var insert = InsertOperation.Create<PocoWithIntPK>(model, data);
			Assert.IsFalse(insert.CanBeBatched);
			var query = insert.GetQuery();
			var builtQuery = new TestQueryConverter().ConvertToQuery(query);

			using (var provider = new SQLite3DataProvider(TestHelper.ConnectionString))
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

				Assert.AreEqual(5, data[0].Id);
				Assert.AreEqual(6, data[1].Id);
				Assert.AreNotEqual(0, data[2].Id);

				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, QueryExpression.Table("PocoWithIntPK"))))
				{
					Assert.IsTrue(queryResult.HasRows);
					var rowCount = 0;
					while (queryResult.Read())
					{
						rowCount++;
						var id = queryResult.GetInt32(0);
						var obj = data.FirstOrDefault(q => q.Id == id);
						Assert.IsNotNull(obj);
						Assert.AreEqual(obj.Data, queryResult.GetString(1));
					}
					Assert.AreEqual(data.Length, rowCount);
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
			Assert.IsTrue(insert.CanBeBatched);
			var query = insert.GetQuery();
			var builtQuery = new TestQueryConverter().ConvertToQuery(query);

			using (var provider = new SQLite3DataProvider(TestHelper.ConnectionString))
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

		[TestMethod]
		public void InsertSingleRelationship()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<RelationshipPoco>();
			schemaBuilder.DefineEntity<PocoWithSingleRelationship>();
			var schema = schemaBuilder.Build();
			var relationshipModel = schema.GetEntityModel<RelationshipPoco>();
			var mainModel = schema.GetEntityModel<PocoWithSingleRelationship>();

			using (var provider = new SQLite3DataProvider(TestHelper.ConnectionString))
			{
				provider.ExecuteNonQuery(QueryExpression.CreateTable(
					nameof(PocoWithSingleRelationship),
					QueryExpression.DefineColumn(nameof(PocoWithSingleRelationship.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(PocoWithSingleRelationship.LocalData), SqlDataType.Text(), isNullable: true),
					QueryExpression.DefineColumn(nameof(PocoWithSingleRelationship.Relationship), SqlDataType.Int(), isNullable: true)
					));
				provider.ExecuteNonQuery(QueryExpression.CreateTable(
					nameof(RelationshipPoco),
					QueryExpression.DefineColumn(nameof(RelationshipPoco.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(RelationshipPoco.RelatedData), SqlDataType.Text(), isNullable: true)
					));

				var relatedObject = new RelationshipPoco
				{
					RelatedData = "World"
				};
				var insert = InsertOperation.Create<RelationshipPoco>(relationshipModel, relatedObject);
				using (var queryResult = provider.ExecuteReader(insert.GetQuery()))
					insert.ProcessResult(queryResult);
				Assert.AreNotEqual(0, relatedObject.Id);

				//  1. insert with the full model objects
				var fullInstance = new PocoWithSingleRelationship
				{
					LocalData = "Hello",
					Relationship = relatedObject
				};
				insert = InsertOperation.Create<PocoWithSingleRelationship>(mainModel, fullInstance);
				using (var queryResult = provider.ExecuteReader(insert.GetQuery()))
					insert.ProcessResult(queryResult);
				Assert.AreNotEqual(0, fullInstance.Id);
				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(
					new[] { QueryExpression.All() }, from: QueryExpression.Table(nameof(PocoWithSingleRelationship))
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());

					Assert.AreEqual(fullInstance.Id, queryResult.GetInt32(0));
					Assert.AreEqual(fullInstance.LocalData, queryResult.GetString(1));
					Assert.AreEqual(relatedObject.Id, queryResult.GetInt32(2));
				}
				provider.ExecuteNonQuery(QueryExpression.Delete(QueryExpression.Table(nameof(PocoWithSingleRelationship))));

				//  2. insert when a projection contains the full related object
				var projectionWithFullRelationship = new PocoWithSingleRelationshipProjection
				{
					LocalData = "Hello",
					Relationship = relatedObject
				};
				insert = InsertOperation.Create(mainModel, projectionWithFullRelationship);
				using (var queryResult = provider.ExecuteReader(insert.GetQuery()))
					insert.ProcessResult(queryResult);
				Assert.AreNotEqual(0, projectionWithFullRelationship.Id);
				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(
					new[] { QueryExpression.All() }, from: QueryExpression.Table(nameof(PocoWithSingleRelationship))
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());

					Assert.AreEqual(projectionWithFullRelationship.Id, queryResult.GetInt32(0));
					Assert.AreEqual(projectionWithFullRelationship.LocalData, queryResult.GetString(1));
					Assert.AreEqual(relatedObject.Id, queryResult.GetInt32(2));
				}
				provider.ExecuteNonQuery(QueryExpression.Delete(QueryExpression.Table(nameof(PocoWithSingleRelationship))));

				//  3. insert when a projection contains a projection of the related object
				var projectionWithProjectedRelationship = new PocoWithSingleRelationshipProjectionProjection
				{
					LocalData = "Hello",
					Relationship = new RelationshipPocoProjection { Id = relatedObject.Id }
				};
				insert = InsertOperation.Create(mainModel, projectionWithProjectedRelationship);
				using (var queryResult = provider.ExecuteReader(insert.GetQuery()))
					insert.ProcessResult(queryResult);
				Assert.AreNotEqual(0, projectionWithProjectedRelationship.Id);
				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(
					new[] { QueryExpression.All() }, from: QueryExpression.Table(nameof(PocoWithSingleRelationship))
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());

					Assert.AreEqual(projectionWithProjectedRelationship.Id, queryResult.GetInt32(0));
					Assert.AreEqual(projectionWithProjectedRelationship.LocalData, queryResult.GetString(1));
					Assert.AreEqual(relatedObject.Id, queryResult.GetInt32(2));
				}
				provider.ExecuteNonQuery(QueryExpression.Delete(QueryExpression.Table(nameof(PocoWithSingleRelationship))));

				//  4. insert when a projection contains a projection of the related objects primary key
				var projectionWithFlatRelationship = new PocoWithFlatSingleRelationshipProjection
				{
					LocalData = "Hello",
					RelationshipId = relatedObject.Id
				};
				insert = InsertOperation.Create(mainModel, projectionWithFlatRelationship);
				using (var queryResult = provider.ExecuteReader(insert.GetQuery()))
					insert.ProcessResult(queryResult);
				Assert.AreNotEqual(0, projectionWithFlatRelationship.Id);
				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(
					new[] { QueryExpression.All() }, from: QueryExpression.Table(nameof(PocoWithSingleRelationship))
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());

					Assert.AreEqual(projectionWithFlatRelationship.Id, queryResult.GetInt32(0));
					Assert.AreEqual(projectionWithFlatRelationship.LocalData, queryResult.GetString(1));
					Assert.AreEqual(relatedObject.Id, queryResult.GetInt32(2));
				}
				provider.ExecuteNonQuery(QueryExpression.Delete(QueryExpression.Table(nameof(PocoWithSingleRelationship))));
			}
		}

		[TestMethod]
		public void InsertManyRelationships()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<RelationshipPoco>();
			schemaBuilder.DefineEntity<PocoWithManyRelationships>();
			var schema = schemaBuilder.Build();
			var relationshipModel = schema.GetEntityModel<RelationshipPoco>();
			var mainModel = schema.GetEntityModel<PocoWithManyRelationships>();

			using (var provider = new SQLite3DataProvider(TestHelper.ConnectionString))
			{
				provider.ExecuteNonQuery(QueryExpression.CreateTable(
					nameof(PocoWithManyRelationships),
					QueryExpression.DefineColumn(nameof(PocoWithSingleRelationship.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(PocoWithSingleRelationship.LocalData), SqlDataType.Text(), isNullable: true)
					));
				provider.ExecuteNonQuery(QueryExpression.CreateTable(
					"PocoWithManyRelationships_RelationshipsToRelationshipPoco",
					QueryExpression.DefineColumn("LocalKey", SqlDataType.Int()),
					QueryExpression.DefineColumn("RemoteKey", SqlDataType.Int())
					));
				provider.ExecuteNonQuery(QueryExpression.CreateTable(
					nameof(RelationshipPoco),
					QueryExpression.DefineColumn(nameof(RelationshipPoco.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(RelationshipPoco.RelatedData), SqlDataType.Text(), isNullable: true)
					));

				var relatedObjects = new[]
				{
					new RelationshipPoco { RelatedData = "Hello" },
					new RelationshipPoco { RelatedData = "World" }
				};
				var insert = InsertOperation.Create<RelationshipPoco>(relationshipModel, relatedObjects);
				using (var queryResult = provider.ExecuteReader(insert.GetQuery()))
					insert.ProcessResult(queryResult);

				Assert.AreNotEqual(0, relatedObjects[0].Id);
				Assert.AreNotEqual(0, relatedObjects[1].Id);
				Assert.AreNotEqual(relatedObjects[0].Id, relatedObjects[1].Id);

				var fullInstance = new PocoWithManyRelationships
				{
					LocalData = "Hello",
					Relationships = { relatedObjects[0], relatedObjects[1] }
				};
				insert = InsertOperation.Create<PocoWithManyRelationships>(mainModel, fullInstance);
				using (var queryResult = provider.ExecuteReader(insert.GetQuery()))
					insert.ProcessResult(queryResult);
				Assert.AreNotEqual(0, fullInstance.Id);
				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table(nameof(PocoWithManyRelationships)),
					joins: new[]
					{
						QueryExpression.Join(
							QueryExpression.Column(nameof(PocoWithManyRelationships.Id), QueryExpression.Table(nameof(PocoWithManyRelationships))),
							QueryExpression.Column("LocalKey", QueryExpression.Table("PocoWithManyRelationships_RelationshipsToRelationshipPoco"))
						),
						QueryExpression.Join(
							QueryExpression.Column("RemoteKey", QueryExpression.Table("PocoWithManyRelationships_RelationshipsToRelationshipPoco")),
							QueryExpression.Column(nameof(RelationshipPoco.Id), QueryExpression.Table(nameof(RelationshipPoco)))
						)
					}
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());

					Assert.AreEqual(fullInstance.Id, queryResult.GetInt32(0));
					Assert.AreEqual(fullInstance.LocalData, queryResult.GetString(1));
					Assert.AreEqual(relatedObjects[0].Id, queryResult.GetInt32(3));
					Assert.AreEqual(relatedObjects[0].RelatedData, queryResult.GetString(5));

					Assert.IsTrue(queryResult.Read());

					Assert.AreEqual(fullInstance.Id, queryResult.GetInt32(0));
					Assert.AreEqual(fullInstance.LocalData, queryResult.GetString(1));
					Assert.AreEqual(relatedObjects[1].Id, queryResult.GetInt32(3));
					Assert.AreEqual(relatedObjects[1].RelatedData, queryResult.GetString(5));
				}
				provider.ExecuteNonQuery(QueryExpression.Delete(QueryExpression.Table(nameof(PocoWithManyRelationships))));
				provider.ExecuteNonQuery(QueryExpression.Delete(QueryExpression.Table("PocoWithManyRelationships_RelationshipsToRelationshipPoco")));

				fullInstance = new PocoWithManyRelationships(5)
				{
					LocalData = "Hello",
					Relationships = { relatedObjects[0], relatedObjects[1] }
				};
				insert = InsertOperation.Create<PocoWithManyRelationships>(mainModel, fullInstance);
				using (var queryResult = provider.ExecuteReader(insert.GetQuery()))
					insert.ProcessResult(queryResult);
				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table(nameof(PocoWithManyRelationships)),
					joins: new[]
					{
						QueryExpression.Join(
							QueryExpression.Column(nameof(PocoWithManyRelationships.Id), QueryExpression.Table(nameof(PocoWithManyRelationships))),
							QueryExpression.Column("LocalKey", QueryExpression.Table("PocoWithManyRelationships_RelationshipsToRelationshipPoco"))
						),
						QueryExpression.Join(
							QueryExpression.Column("RemoteKey", QueryExpression.Table("PocoWithManyRelationships_RelationshipsToRelationshipPoco")),
							QueryExpression.Column(nameof(RelationshipPoco.Id), QueryExpression.Table(nameof(RelationshipPoco)))
						)
					}
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());

					Assert.AreEqual(fullInstance.Id, queryResult.GetInt32(0));
					Assert.AreEqual(fullInstance.LocalData, queryResult.GetString(1));
					Assert.AreEqual(relatedObjects[0].Id, queryResult.GetInt32(3));
					Assert.AreEqual(relatedObjects[0].RelatedData, queryResult.GetString(5));

					Assert.IsTrue(queryResult.Read());

					Assert.AreEqual(fullInstance.Id, queryResult.GetInt32(0));
					Assert.AreEqual(fullInstance.LocalData, queryResult.GetString(1));
					Assert.AreEqual(relatedObjects[1].Id, queryResult.GetInt32(3));
					Assert.AreEqual(relatedObjects[1].RelatedData, queryResult.GetString(5));
				}
				provider.ExecuteNonQuery(QueryExpression.Delete(QueryExpression.Table(nameof(PocoWithManyRelationships))));
				provider.ExecuteNonQuery(QueryExpression.Delete(QueryExpression.Table("PocoWithManyRelationships_RelationshipsToRelationshipPoco")));
			}
		}

		[TestMethod]
		public void InsertEmbeddedObject()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PocoWithSingleRelationship>();
			var schema = schemaBuilder.Build();
			var mainModel = schema.GetEntityModel<PocoWithSingleRelationship>();

			using (var provider = new SQLite3DataProvider(TestHelper.ConnectionString))
			{
				provider.ExecuteNonQuery(QueryExpression.CreateTable(
					nameof(PocoWithSingleRelationship),
					QueryExpression.DefineColumn(nameof(PocoWithSingleRelationship.Id), SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn(nameof(PocoWithSingleRelationship.LocalData), SqlDataType.Text(), isNullable: true),
					QueryExpression.DefineColumn(nameof(PocoWithSingleRelationship.Relationship), SqlDataType.Int()),
					QueryExpression.DefineColumn(nameof(PocoWithSingleRelationship.Relationship) + "_" + nameof(RelationshipPoco.Id), SqlDataType.Int()),
					QueryExpression.DefineColumn(nameof(PocoWithSingleRelationship.Relationship) + "_" + nameof(RelationshipPoco.RelatedData), SqlDataType.Text(), isNullable: true)
					));

				var fullInstance = new PocoWithSingleRelationship
				{
					LocalData = "Hello",
					Relationship = new RelationshipPoco
					{
						RelatedData = "World"
					}
				};
				var insert = InsertOperation.Create<PocoWithSingleRelationship>(mainModel, fullInstance);
				using (var queryResult = provider.ExecuteReader(insert.GetQuery()))
					insert.ProcessResult(queryResult);
				Assert.AreNotEqual(0, fullInstance.Id);
				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(
					new[] { QueryExpression.All() }, from: QueryExpression.Table(nameof(PocoWithSingleRelationship))
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());

					Assert.AreEqual(fullInstance.Id, queryResult.GetInt32(0));
					Assert.AreEqual(fullInstance.LocalData, queryResult.GetString(1));
					Assert.AreEqual(1, queryResult.GetInt32(2));
					Assert.AreEqual(fullInstance.Relationship.Id, queryResult.GetInt32(3));
					Assert.AreEqual(fullInstance.Relationship.RelatedData, queryResult.GetString(4));
				}
				provider.ExecuteNonQuery(QueryExpression.Delete(QueryExpression.Table(nameof(PocoWithSingleRelationship))));

				var projectionWithFullRelationship = new PocoWithSingleRelationshipProjection
				{
					LocalData = "Hello",
					Relationship = new RelationshipPoco
					{
						RelatedData = "World"
					}
				};
				insert = InsertOperation.Create(mainModel, projectionWithFullRelationship);
				using (var queryResult = provider.ExecuteReader(insert.GetQuery()))
					insert.ProcessResult(queryResult);
				Assert.AreNotEqual(0, projectionWithFullRelationship.Id);
				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(
					new[] { QueryExpression.All() }, from: QueryExpression.Table(nameof(PocoWithSingleRelationship))
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());

					Assert.AreEqual(projectionWithFullRelationship.Id, queryResult.GetInt32(0));
					Assert.AreEqual(projectionWithFullRelationship.LocalData, queryResult.GetString(1));
					Assert.AreEqual(1, queryResult.GetInt32(2));
					Assert.AreEqual(projectionWithFullRelationship.Relationship.Id, queryResult.GetInt32(3));
					Assert.AreEqual(fullInstance.Relationship.RelatedData, queryResult.GetString(4));
				}
				provider.ExecuteNonQuery(QueryExpression.Delete(QueryExpression.Table(nameof(PocoWithSingleRelationship))));

				var projectionWithProjectedRelationship = new PocoWithSingleRelationshipProjectionProjection
				{
					LocalData = "Hello",
					Relationship = new RelationshipPocoProjection { Id = 2 }
				};
				insert = InsertOperation.Create(mainModel, projectionWithProjectedRelationship);
				using (var queryResult = provider.ExecuteReader(insert.GetQuery()))
					insert.ProcessResult(queryResult);
				Assert.AreNotEqual(0, projectionWithProjectedRelationship.Id);
				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(
					new[] { QueryExpression.All() }, from: QueryExpression.Table(nameof(PocoWithSingleRelationship))
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());

					Assert.AreEqual(projectionWithProjectedRelationship.Id, queryResult.GetInt32(0));
					Assert.AreEqual(projectionWithProjectedRelationship.LocalData, queryResult.GetString(1));
					Assert.AreEqual(1, queryResult.GetInt32(2));
					Assert.AreEqual(projectionWithProjectedRelationship.Relationship.Id, queryResult.GetInt32(3));
					Assert.IsTrue(queryResult.IsDBNull(4));
				}
				provider.ExecuteNonQuery(QueryExpression.Delete(QueryExpression.Table(nameof(PocoWithSingleRelationship))));

				var projectionWithFlatRelationship = new PocoWithFlatSingleRelationshipProjection
				{
					LocalData = "Hello",
					RelationshipId = 3
				};
				insert = InsertOperation.Create(mainModel, projectionWithFlatRelationship);
				using (var queryResult = provider.ExecuteReader(insert.GetQuery()))
					insert.ProcessResult(queryResult);
				Assert.AreNotEqual(0, projectionWithFlatRelationship.Id);
				using (var queryResult = provider.ExecuteReader(QueryExpression.Select(
					new[] { QueryExpression.All() }, from: QueryExpression.Table(nameof(PocoWithSingleRelationship))
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());

					Assert.AreEqual(projectionWithFlatRelationship.Id, queryResult.GetInt32(0));
					Assert.AreEqual(projectionWithFlatRelationship.LocalData, queryResult.GetString(1));
					Assert.AreEqual(1, queryResult.GetInt32(2));
					Assert.AreEqual(projectionWithFlatRelationship.RelationshipId, queryResult.GetInt32(3));
					Assert.IsTrue(queryResult.IsDBNull(4));
				}
				provider.ExecuteNonQuery(QueryExpression.Delete(QueryExpression.Table(nameof(PocoWithSingleRelationship))));
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

		private class PocoWithSingleRelationship
		{
			public int Id { get; private set; }
			public string LocalData { get; set; }
			public RelationshipPoco Relationship { get; set; }
		}

		private class RelationshipPoco
		{
			public int Id { get; private set; }
			public string RelatedData { get; set; }
		}

		private class RelationshipPocoProjection
		{
			public int Id { get; set; }
		}

		private class PocoWithSingleRelationshipProjection
		{
			public int Id { get; private set; }
			public string LocalData { get; set; }
			public RelationshipPoco Relationship { get; set; }
		}

		private class PocoWithSingleRelationshipProjectionProjection
		{
			public int Id { get; private set; }
			public string LocalData { get; set; }
			public RelationshipPocoProjection Relationship { get; set; }
		}

		private class PocoWithFlatSingleRelationshipProjection
		{
			public int Id { get; private set; }
			public string LocalData { get; set; }
			public int RelationshipId { get; set; }
		}

		private class PocoWithManyRelationships
		{
			public int Id { get; private set; }
			public string LocalData { get; set; }
			public List<RelationshipPoco> Relationships { get; set; }
				= new List<RelationshipPoco>();

			public PocoWithManyRelationships() { }
			public PocoWithManyRelationships(int id) { Id = id; }
		}
	}
}
