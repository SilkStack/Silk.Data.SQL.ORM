using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class ModelSelectTests
	{
		[TestMethod]
		public async Task SelectFlatModel()
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
				queryBuilder.Project<FlatEntity>();
				var selectQuery = queryBuilder.BuildQuery();

				using (var queryResult = await provider.ExecuteReaderAsync(selectQuery))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());
					Assert.AreEqual(1, queryResult.GetInt32(queryResult.GetOrdinal(nameof(FlatEntity.Id))));
					Assert.AreEqual(2, queryResult.GetInt32(queryResult.GetOrdinal(nameof(FlatEntity.Data))));
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
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("CREATE TABLE [FlatEntity] ([Id] INT, [Data] INT)")
					);
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("CREATE TABLE [RelationshipEntity] ([FK_Child_Id] INT, [Data] INT)")
					);
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("INSERT INTO [FlatEntity] VALUES (1, 2)")
					);
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("INSERT INTO [RelationshipEntity] VALUES (1, 3)")
					);

				var queryBuilder = new EntitySelectBuilder<RelationshipEntity>(schema);
				queryBuilder.Project<RelationshipEntity>();
				var selectQuery = queryBuilder.BuildQuery();

				using (var queryResult = await provider.ExecuteReaderAsync(selectQuery))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());
					Assert.AreEqual(1, queryResult.GetInt32(queryResult.GetOrdinal("Child_Id")));
					Assert.AreEqual(2, queryResult.GetInt32(queryResult.GetOrdinal("Child_Data")));
					Assert.AreEqual(3, queryResult.GetInt32(queryResult.GetOrdinal("Data")));
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
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("CREATE TABLE [RelationshipEntity] ([Data] INT, [Child_Id] INT, [Child_Data] INT)")
					);
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("INSERT INTO [RelationshipEntity] VALUES (3, 1, 2)")
					);

				var queryBuilder = new EntitySelectBuilder<RelationshipEntity>(schema);
				queryBuilder.Project<RelationshipEntity>();
				var selectQuery = queryBuilder.BuildQuery();

				using (var queryResult = await provider.ExecuteReaderAsync(selectQuery))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());
					Assert.AreEqual(1, queryResult.GetInt32(queryResult.GetOrdinal("Child_Id")));
					Assert.AreEqual(2, queryResult.GetInt32(queryResult.GetOrdinal("Child_Data")));
					Assert.AreEqual(3, queryResult.GetInt32(queryResult.GetOrdinal("Data")));
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
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("CREATE TABLE [DeepRelationshipEntity] ([Child] INT, [Child_Data] INT, [FK_Child_Id] INT)")
					);
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("CREATE TABLE [FlatEntity] ([Id] INT, [Data] INT)")
					);
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("INSERT INTO [FlatEntity] VALUES (1, 2)")
					);
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("INSERT INTO [DeepRelationshipEntity] VALUES (1, 3, 1)")
					);

				var queryBuilder = new EntitySelectBuilder<DeepRelationshipEntity>(schema);
				queryBuilder.Project<DeepRelationshipEntity>();
				var selectQuery = queryBuilder.BuildQuery();

				using (var queryResult = await provider.ExecuteReaderAsync(selectQuery))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());
					Assert.AreEqual(1, queryResult.GetInt32(queryResult.GetOrdinal("Child_Child_Id")));
					Assert.AreEqual(2, queryResult.GetInt32(queryResult.GetOrdinal("Child_Child_Data")));
					Assert.AreEqual(3, queryResult.GetInt32(queryResult.GetOrdinal("Child_Data")));
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
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("CREATE TABLE [DeepRelationshipEntity] ([FK_Child_Child_Id] INT)")
					);
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("CREATE TABLE [RelationshipEntity] ([Data] INT, [Child_Id] INT, [Child_Data] INT)")
					);
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("INSERT INTO [RelationshipEntity] VALUES (3, 1, 2)")
					);
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("INSERT INTO [DeepRelationshipEntity] VALUES (1)")
					);

				var queryBuilder = new EntitySelectBuilder<DeepRelationshipEntity>(schema);
				queryBuilder.Project<DeepRelationshipEntity>();
				var selectQuery = queryBuilder.BuildQuery();

				using (var queryResult = await provider.ExecuteReaderAsync(selectQuery))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());
					Assert.AreEqual(1, queryResult.GetInt32(queryResult.GetOrdinal("Child_Child_Id")));
					Assert.AreEqual(2, queryResult.GetInt32(queryResult.GetOrdinal("Child_Child_Data")));
					Assert.AreEqual(3, queryResult.GetInt32(queryResult.GetOrdinal("Child_Data")));
				}
			}
		}

		private class FlatEntity
		{
			public int Id { get; private set; }
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
