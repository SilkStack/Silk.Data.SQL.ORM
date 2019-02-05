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

				var selectBuilder = new EntitySelectBuilder<FlatEntity>(schema);
				var mapper = selectBuilder.Projection.AddClass<FlatEntity>();
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

				var queryBuilder = new EntitySelectBuilder<RelationshipEntity>(schema);
				var mapper = queryBuilder.Projection.AddClass<RelationshipEntity>();

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

				var queryBuilder = new EntitySelectBuilder<RelationshipEntity>(schema);
				var mapper = queryBuilder.Projection.AddClass<RelationshipEntity>();
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

				var queryBuilder = new EntitySelectBuilder<DeepRelationshipEntity>(schema);
				var mapper = queryBuilder.Projection.AddClass<DeepRelationshipEntity>();
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

				var queryBuilder = new EntitySelectBuilder<DeepRelationshipEntity>(schema);
				var mapper = queryBuilder.Projection.AddClass<DeepRelationshipEntity>();
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

				var queryBuilder = new EntitySelectBuilder<FlatEntity>(schema);
				var mapper = queryBuilder.Projection.AddField(q => Alias(Count(q.Id), "count"));
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

				var queryBuilder = new EntitySelectBuilder<FlatEntity>(schema);
				var mapper = queryBuilder.Projection.AddField(q => q.Data);
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

		[TestMethod]
		public async Task SelectDeepParent()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<Child>();
			schemaBuilder.DefineEntity<Parent>();
			schemaBuilder.DefineEntity<DeepParent>();
			var schema = schemaBuilder.Build();

			var inObj = new DeepParent
			{
				Child = new Parent
				{
					Child = new Child
					{
						Data = "Hello"
					}
				}
			};

			using (var provider = TestHelper.CreateProvider())
			{
				await provider.ExecuteAsync(
					schema.CreateTable<Child>(),
					schema.CreateTable<Parent>(),
					schema.CreateTable<DeepParent>(),
					schema.CreateInsert(inObj.Child.Child),
					schema.CreateInsert(inObj.Child),
					schema.CreateInsert(inObj)
					);

				var select = schema.CreateSelect<DeepParent>();
				await provider.ExecuteAsync(select);

				Assert.AreEqual(1, select.Result.Count);
				var outObj = select.Result.First();
				Assert.AreEqual(inObj.Id, outObj.Id);
				Assert.AreEqual(inObj.Child.Id, outObj.Child.Id);
				Assert.AreEqual(inObj.Child.Child.Id, outObj.Child.Child.Id);
				Assert.AreEqual(inObj.Child.Child.Data, outObj.Child.Child.Data);
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
			var createSchema = new CreateEntityTableBuilder<T>(schema);
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

		private class Parent
		{
			public Guid Id { get; private set; }
			public Child Child { get; set; }
		}

		private class Child
		{
			public Guid Id { get; private set; }
			public string Data { get; set; }
		}

		private class DeepParent
		{
			public Guid Id { get; private set; }
			public Parent Child { get; set; }
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
