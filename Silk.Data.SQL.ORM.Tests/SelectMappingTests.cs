using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Providers;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Silk.Data.SQL.ORM.DatabaseFunctions;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class SelectMappingTests
	{
		[TestMethod]
		public async Task SelectFlatModel()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<FlatEntity>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await CreateSchema<FlatEntity>(schema, provider);

				var inObj = new FlatEntity { Data = 2 };

				await Insert(schema, provider, inObj);

				var selectBuilder = new SelectBuilder<FlatEntity>(schema);
				var mapper = selectBuilder.Project<FlatEntity>();
				var query = selectBuilder.BuildQuery();

				using (var queryResult = await provider.ExecuteReaderAsync(query))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					var outObj = mapper.Map(queryResult);
					Assert.IsNotNull(outObj);
					Assert.AreEqual(inObj.Id, outObj.Id);
					Assert.AreEqual(inObj.Data, outObj.Data);
				}
			}
		}

		[TestMethod]
		public async Task SelectRelationshipModel()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<FlatEntity>();
			schemaBuilder.DefineEntity<RelationshipEntity>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await CreateSchema<FlatEntity>(schema, provider);
				await CreateSchema<RelationshipEntity>(schema, provider);

				var inFlat = new FlatEntity { Data = 2 };
				var inRelated = new RelationshipEntity { Data = 3, Child = inFlat };

				await Insert(schema, provider, inFlat);
				await Insert(schema, provider, inRelated);

				var queryBuilder = new SelectBuilder<RelationshipEntity>(schema);
				var mapper = queryBuilder.Project<RelationshipEntity>();

				using (var queryResult = await provider.ExecuteReaderAsync(queryBuilder.BuildQuery()))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());
					var outRelated = mapper.Map(queryResult);
					Assert.IsNotNull(outRelated);
					Assert.IsNotNull(outRelated.Child);
					Assert.AreEqual(inRelated.Data, outRelated.Data);
					Assert.AreEqual(inRelated.Child.Data, outRelated.Child.Data);
				}
			}
		}

		[TestMethod]
		public async Task SelectEmbeddedModel()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<RelationshipEntity>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await CreateSchema<RelationshipEntity>(schema, provider);

				var inObj = new RelationshipEntity
				{
					Data = 2,
					Child = new FlatEntity
					{
						Data = 3
					}
				};

				await Insert(schema, provider, inObj);

				var queryBuilder = new SelectBuilder<RelationshipEntity>(schema);
				var mapper = queryBuilder.Project<RelationshipEntity>();
				var selectQuery = queryBuilder.BuildQuery();

				using (var queryResult = await provider.ExecuteReaderAsync(selectQuery))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					var outObj = mapper.Map(queryResult);
					Assert.IsNotNull(outObj);
					Assert.IsNotNull(outObj.Child);
					Assert.AreEqual(inObj.Data, outObj.Data);
					Assert.AreEqual(inObj.Child.Data, outObj.Child.Data);
				}
			}
		}

		[TestMethod]
		public async Task SelectEmbeddedObjectWithRelationshipModel()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<DeepRelationshipEntity>();
			schemaBuilder.DefineEntity<FlatEntity>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await CreateSchema<DeepRelationshipEntity>(schema, provider);
				await CreateSchema<FlatEntity>(schema, provider);

				var inObj = new DeepRelationshipEntity
				{
					Child = new RelationshipEntity
					{
						Data = 2,
						Child = new FlatEntity
						{
							Data = 3
						}
					}
				};
				await Insert(schema, provider, inObj.Child.Child);
				await Insert(schema, provider, inObj);

				var queryBuilder = new SelectBuilder<DeepRelationshipEntity>(schema);
				var mapper = queryBuilder.Project<DeepRelationshipEntity>();
				var selectQuery = queryBuilder.BuildQuery();

				using (var queryResult = await provider.ExecuteReaderAsync(selectQuery))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					var outObj = mapper.Map(queryResult);
					Assert.IsNotNull(outObj);
					Assert.IsNotNull(outObj.Child);
					Assert.IsNotNull(outObj.Child.Child);
					Assert.AreEqual(inObj.Child.Data, outObj.Child.Data);
					Assert.AreEqual(inObj.Child.Child.Data, outObj.Child.Child.Data);
				}
			}
		}

		[TestMethod]
		public async Task SelectRelationshipWithEmbeddedObjectModel()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<DeepRelationshipEntity>();
			schemaBuilder.DefineEntity<RelationshipEntity>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await CreateSchema<DeepRelationshipEntity>(schema, provider);
				await CreateSchema<RelationshipEntity>(schema, provider);

				var inObj = new DeepRelationshipEntity
				{
					Child = new RelationshipEntity
					{
						Data = 2,
						Child = new FlatEntity
						{
							Data = 3
						}
					}
				};
				await Insert(schema, provider, inObj.Child);
				await Insert(schema, provider, inObj);

				var queryBuilder = new SelectBuilder<DeepRelationshipEntity>(schema);
				var mapper = queryBuilder.Project<DeepRelationshipEntity>();
				var selectQuery = queryBuilder.BuildQuery();

				using (var queryResult = await provider.ExecuteReaderAsync(selectQuery))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					var outObj = mapper.Map(queryResult);
					Assert.IsNotNull(outObj);
					Assert.IsNotNull(outObj.Child);
					Assert.IsNotNull(outObj.Child.Child);
					Assert.AreEqual(inObj.Child.Data, outObj.Child.Data);
					Assert.AreEqual(inObj.Child.Child.Data, outObj.Child.Child.Data);
				}
			}
		}

		[TestMethod]
		public async Task SelectCountExpression()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<FlatEntity>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("CREATE TABLE [FlatEntity] ([Id] INT, [Data] INT)")
					);
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("INSERT INTO [FlatEntity] VALUES (1, 2)")
					);

				var queryBuilder = new SelectBuilder<FlatEntity>(schema);
				var mapper = queryBuilder.Project(q => Alias(Count(q.Id), "count"));
				Assert.IsNotNull(mapper);
				var selectQuery = queryBuilder.BuildQuery();

				using (var queryResult = await provider.ExecuteReaderAsync(selectQuery))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());
					var result = mapper.Read(queryResult);
					Assert.AreEqual(1, result);
				}
			}
		}

		[TestMethod]
		public async Task SelectFlatFieldExpression()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<FlatEntity>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("CREATE TABLE [FlatEntity] ([Id] INT, [Data] INT)")
					);
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("INSERT INTO [FlatEntity] VALUES (1, 2)")
					);

				var queryBuilder = new SelectBuilder<FlatEntity>(schema);
				var mapper = queryBuilder.Project(q => q.Data);
				Assert.IsNotNull(mapper);

				var selectQuery = queryBuilder.BuildQuery();

				using (var queryResult = await provider.ExecuteReaderAsync(selectQuery))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());
					var result = mapper.Read(queryResult);
					Assert.AreEqual(2, result);
				}
			}
		}

		[TestMethod]
		public async Task SelectRelationship()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<FlatEntity>();
			schemaBuilder.DefineEntity<FlatEntityTwo>();
			schemaBuilder.DefineRelationship<FlatEntity, FlatEntityTwo>("Relationship");
			var schema = schemaBuilder.Build();

			var relationship = schema.GetRelationship<FlatEntity, FlatEntityTwo>("Relationship");

			var inFlatOnes = new[]
			{
				new FlatEntity { Data = 1 },
				new FlatEntity { Data = 2 },
				new FlatEntity { Data = 3 }
			};
			var inFlatTwos = new[]
			{
				new FlatEntityTwo { Data = 4 },
				new FlatEntityTwo { Data = 5 },
				new FlatEntityTwo { Data = 6 }
			};

			using (var provider = TestHelper.CreateProvider())
			{
				await provider.ExecuteAsync(
					schema.CreateTable<FlatEntity>(),
					schema.CreateTable<FlatEntityTwo>(),
					relationship.CreateTable(),
					schema.CreateInsert(inFlatOnes),
					schema.CreateInsert(inFlatTwos),
					relationship.CreateInsert(inFlatOnes[0], inFlatTwos[0]),
					relationship.CreateInsert(inFlatOnes[1], inFlatTwos[0], inFlatTwos[1]),
					relationship.CreateInsert(inFlatOnes[2], inFlatTwos[0], inFlatTwos[1], inFlatTwos[2])
					);

				var selectQuery = relationship.CreateSelect();
				await provider.ExecuteAsync(selectQuery);

				var resultSet = selectQuery.Result;

				Assert.AreEqual(6, resultSet.Count);
				Assert.IsTrue(resultSet.Any(q => q.Item1.Data == 1 && q.Item2.Data == 4));
				Assert.IsTrue(resultSet.Any(q => q.Item1.Data == 2 && q.Item2.Data == 4));
				Assert.IsTrue(resultSet.Any(q => q.Item1.Data == 2 && q.Item2.Data == 5));
				Assert.IsTrue(resultSet.Any(q => q.Item1.Data == 3 && q.Item2.Data == 4));
				Assert.IsTrue(resultSet.Any(q => q.Item1.Data == 3 && q.Item2.Data == 5));
				Assert.IsTrue(resultSet.Any(q => q.Item1.Data == 3 && q.Item2.Data == 6));
			}
		}

		[TestMethod]
		public async Task SelectRelatedRelationship()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<FlatEntity>();
			schemaBuilder.DefineEntity<FlatEntityTwo>();
			schemaBuilder.DefineEntity<RelationshipEntityTwo>();
			schemaBuilder.DefineRelationship<FlatEntityTwo, RelationshipEntityTwo>("Relationship");
			var schema = schemaBuilder.Build();

			var relationship = schema.GetRelationship<FlatEntityTwo, RelationshipEntityTwo>("Relationship");

			using (var provider = TestHelper.CreateProvider())
			{
				var inObjTwos = new[]
				{
					new FlatEntityTwo { Data = 1 }
				};
				var inObjs = new[]
				{
					new FlatEntity { Data = 2 }
				};
				var inRelationships = new[]
				{
					new RelationshipEntityTwo { Data = 3, Child = inObjs[0] }
				};

				await provider.ExecuteAsync(
					schema.CreateTable<FlatEntity>(),
					schema.CreateTable<FlatEntityTwo>(),
					schema.CreateTable<RelationshipEntityTwo>(),
					relationship.CreateTable(),
					schema.CreateInsert(inObjs),
					schema.CreateInsert(inObjTwos),
					schema.CreateInsert(inRelationships),
					relationship.CreateInsert(inObjTwos[0], inRelationships[0])
					);

				var selectQuery = relationship.CreateSelect();
				await provider.ExecuteAsync(selectQuery);

				var result = selectQuery.Result;
				Assert.AreEqual(1, result.Count);
				var relatedData = result.First();
				Assert.AreEqual(inObjTwos[0].Id, relatedData.Item1.Id);
				Assert.AreEqual(inObjTwos[0].Data, relatedData.Item1.Data);
				Assert.AreEqual(inRelationships[0].Id, relatedData.Item2.Id);
				Assert.AreEqual(inRelationships[0].Data, relatedData.Item2.Data);
				Assert.AreEqual(inRelationships[0].Child.Id, relatedData.Item2.Child.Id);
				Assert.AreEqual(inRelationships[0].Child.Data, relatedData.Item2.Child.Data);
			}
		}

		[TestMethod]
		public async Task SelectRelationshipCount()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<FlatEntity>();
			schemaBuilder.DefineEntity<FlatEntityTwo>();
			schemaBuilder.DefineRelationship<FlatEntity, FlatEntityTwo>("Relationship");
			var schema = schemaBuilder.Build();

			var relationship = schema.GetRelationship<FlatEntity, FlatEntityTwo>("Relationship");

			var inFlatOnes = new[]
			{
				new FlatEntity { Data = 1 },
				new FlatEntity { Data = 2 },
				new FlatEntity { Data = 3 }
			};
			var inFlatTwos = new[]
			{
				new FlatEntityTwo { Data = 4 },
				new FlatEntityTwo { Data = 5 },
				new FlatEntityTwo { Data = 6 }
			};

			using (var provider = TestHelper.CreateProvider())
			{
				await provider.ExecuteAsync(
					schema.CreateTable<FlatEntity>(),
					schema.CreateTable<FlatEntityTwo>(),
					relationship.CreateTable(),
					schema.CreateInsert(inFlatOnes),
					schema.CreateInsert(inFlatTwos),
					relationship.CreateInsert(inFlatOnes[0], inFlatTwos[0]),
					relationship.CreateInsert(inFlatOnes[1], inFlatTwos[0], inFlatTwos[1]),
					relationship.CreateInsert(inFlatOnes[2], inFlatTwos[0], inFlatTwos[1], inFlatTwos[2])
					);

				var selectQuery = relationship.CreateCount();
				await provider.ExecuteAsync(selectQuery);

				var resultSet = selectQuery.Result;

				Assert.AreEqual(1, resultSet.Count);
				Assert.AreEqual(6, resultSet.First());
			}
		}

		[TestMethod]
		public async Task SelectEntityWithEnum()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<FlatEntityWithEnum>();
			var schema = schemaBuilder.Build();

			var inObj = new FlatEntityWithEnum { Enum = EnumType.Two };

			using (var provider = TestHelper.CreateProvider())
			{
				await provider.ExecuteAsync(
					schema.CreateTable<FlatEntityWithEnum>(),
					schema.CreateInsert(inObj)
					);

				var outObj = default(FlatEntityWithEnum);
				var select = schema.CreateSelect<FlatEntityWithEnum>();
				await provider.ExecuteAsync(
					select.WithFirstResult(r => outObj = r)
					);
				Assert.IsNotNull(outObj);
				Assert.AreEqual(inObj.Enum, outObj.Enum);
			}
		}

		private Task Insert<T>(Schema.Schema schema, IDataProvider provider, T obj)
			where T : class
		{
			return provider.ExecuteAsync(schema.CreateInsert(obj));
		}

		private async Task CreateSchema<T>(Schema.Schema schema, IDataProvider dataProvider)
			where T : class
		{
			var createSchema = new CreateTableBuilder<T>(schema);
			await dataProvider.ExecuteNonQueryAsync(createSchema.BuildQuery());
		}

		private class FlatEntity
		{
			public Guid Id { get; private set; }
			public int Data { get; set; }
		}

		private class FlatEntityTwo
		{
			public Guid Id { get; private set; }
			public int Data { get; set; }
		}

		private class RelationshipEntity
		{
			public FlatEntity Child { get; set; }
			public int Data { get; set; }
		}

		private class RelationshipEntityTwo
		{
			public Guid Id { get; private set; }
			public FlatEntity Child { get; set; }
			public int Data { get; set; }
		}

		private class DeepRelationshipEntity
		{
			public RelationshipEntity Child { get; set; }
		}

		private class FlatEntityWithEnum
		{
			public EnumType Enum { get; set; }
		}

		private enum EnumType
		{
			One,
			Two,
			Three
		}
	}
}
