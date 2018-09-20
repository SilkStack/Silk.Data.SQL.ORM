﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using System.Threading.Tasks;
using static Silk.Data.SQL.ORM.DatabaseFunctions;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class SelectQueryTests
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

				var queryBuilder = new SelectBuilder<FlatEntity>(schema);
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

				var queryBuilder = new SelectBuilder<RelationshipEntity>(schema);
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
					SQLite3.SQLite3.Raw("CREATE TABLE [RelationshipEntity] ([Data] INT, [Child] INT, [Child_Id] INT, [Child_Data] INT)")
					);
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("INSERT INTO [RelationshipEntity] VALUES (3, 1, 1, 2)")
					);

				var queryBuilder = new SelectBuilder<RelationshipEntity>(schema);
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

				var queryBuilder = new SelectBuilder<DeepRelationshipEntity>(schema);
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
					SQLite3.SQLite3.Raw("CREATE TABLE [RelationshipEntity] ([Data] INT, [Child] INT, [Child_Id] INT, [Child_Data] INT)")
					);
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("INSERT INTO [RelationshipEntity] VALUES (3, 1, 1, 2)")
					);
				await provider.ExecuteNonQueryAsync(
					SQLite3.SQLite3.Raw("INSERT INTO [DeepRelationshipEntity] VALUES (1)")
					);

				var queryBuilder = new SelectBuilder<DeepRelationshipEntity>(schema);
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
				queryBuilder.Project(q => Alias(Count(q.Id), "count"));
				var selectQuery = queryBuilder.BuildQuery();

				using (var queryResult = await provider.ExecuteReaderAsync(selectQuery))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());
					Assert.AreEqual(1, queryResult.GetInt32(queryResult.GetOrdinal("count")));
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
				queryBuilder.Project(q => q.Data);
				var selectQuery = queryBuilder.BuildQuery();

				using (var queryResult = await provider.ExecuteReaderAsync(selectQuery))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());
					Assert.AreEqual(2, queryResult.GetInt32(queryResult.GetOrdinal(nameof(FlatEntity.Data))));

					try
					{
						queryResult.GetOrdinal(nameof(FlatEntity.Id));
						Assert.Fail("Id field was included in projection.");
					}
					catch { }
				}
			}
		}

		[TestMethod]
		public async Task SelectWhereInArray()
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
					SQLite3.SQLite3.Raw("INSERT INTO [FlatEntity] VALUES (1, 2), (2, 3)")
					);

				var queryBuilder = new SelectBuilder<FlatEntity>(schema);
				queryBuilder.Project<FlatEntity>();
				var ids = new[] { 1, 2 };
				queryBuilder.AndWhere(q => IsIn(q.Id, ids));
				var selectQuery = queryBuilder.BuildQuery();

				using (var queryResult = await provider.ExecuteReaderAsync(selectQuery))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());
					Assert.IsTrue(await queryResult.ReadAsync());
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