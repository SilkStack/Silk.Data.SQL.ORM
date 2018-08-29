using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Providers;
using System;
using System.Threading.Tasks;

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

				var queryBuilder = new EntitySelectBuilder<RelationshipEntity>(schema);
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

				var queryBuilder = new EntitySelectBuilder<RelationshipEntity>(schema);
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

				var queryBuilder = new EntitySelectBuilder<DeepRelationshipEntity>(schema);
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

				var queryBuilder = new EntitySelectBuilder<DeepRelationshipEntity>(schema);
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

		private Task Insert<T>(Schema.Schema schema, IDataProvider provider, T obj)
			where T : class
		{
			var builder = new EntityInsertBuilder<T>(schema);
			builder.Add(obj);
			return provider.ExecuteNonQueryAsync(builder.BuildQuery());
		}

		private async Task CreateSchema<T>(Schema.Schema schema, IDataProvider dataProvider)
			where T : class
		{
			var createSchema = new EntityCreateSchemaBuilder<T>(schema);
			await dataProvider.ExecuteNonQueryAsync(createSchema.BuildQuery());
		}

		private class FlatEntity
		{
			public Guid Id { get; private set; }
			public int Data { get; set; }
		}

		private class RelationshipEntity
		{
			public FlatEntity Child { get; set; }
			public int Data { get; set; }
		}

		private class DeepRelationshipEntity
		{
			public RelationshipEntity Child { get; set; }
		}
	}
}
