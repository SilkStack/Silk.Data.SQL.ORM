using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Operations;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.SQLite3;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class DatabaseTests
	{
		[TestMethod]
		public void SelectEnum()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PocoWithEnum>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<PocoWithEnum>();
			using (var dataProvider = new SQLite3DataProvider(TestHelper.ConnectionString))
			{
				dataProvider.ExecuteNonReader(CreateTableOperation.Create(model.EntityTable));

				var entities = new[]
				{
					new PocoWithEnum { SimpleEnum = SimpleEnum.One },
					new PocoWithEnum { SimpleEnum = SimpleEnum.Two },
					new PocoWithEnum { SimpleEnum = SimpleEnum.Three }
				};

				var database = new EntityDatabase<PocoWithEnum>(schema, dataProvider);

				database.Insert(entities);

				var queriedEntities = database.Query(
					where: database.Condition(q => q.SimpleEnum != SimpleEnum.Two).Build(),
					orderBy: database.OrderByDescending(q => q.SimpleEnum).Build()
					);
				Assert.AreEqual(2, queriedEntities.Count);
				Assert.AreEqual(SimpleEnum.Three, queriedEntities.First().SimpleEnum);
				Assert.AreEqual(SimpleEnum.One, queriedEntities.Skip(1).First().SimpleEnum);
			}
		}

		[TestMethod]
		public void Count()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<SimplePoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<SimplePoco>();
			using (var dataProvider = new SQLite3DataProvider(TestHelper.ConnectionString))
			{
				dataProvider.ExecuteNonReader(CreateTableOperation.Create(model.EntityTable));

				var entities = new[]
				{
					new SimplePoco { Data = "Hello" },
					new SimplePoco { Data = "World" }
				};

				var database = new EntityDatabase<SimplePoco>(schema, dataProvider);

				database.Insert(entities);

				var count = database.Count(
					where: database
						.Condition(q => q.Id == entities[0].Id || q.Id == entities[1].Id)
						.Build()
					);
				Assert.AreEqual(entities.Length, count);
			}
		}

		[TestMethod]
		public void InsertAndSelect()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<SimplePoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<SimplePoco>();
			using (var dataProvider = new SQLite3DataProvider(TestHelper.ConnectionString))
			{
				dataProvider.ExecuteNonReader(CreateTableOperation.Create(model.EntityTable));

				var entities = new[]
				{
					new SimplePoco { Data = "Hello" },
					new SimplePoco { Data = "World" }
				};

				var database = new EntityDatabase<SimplePoco>(schema, dataProvider);

				database.Insert(entities);

				var retrievedEntities = database.Query(
					where: database
						.Condition(q => q.Id == entities[0].Id || q.Id == entities[1].Id)
						.Build(),
					orderBy: database.OrderByDescending(q => q.Id).Build()
					);

				Assert.AreEqual(entities.Length, retrievedEntities.Count);
				foreach (var entity in entities)
				{
					Assert.AreNotEqual(Guid.Empty, entity.Id);
					var retrievedEntity = retrievedEntities.FirstOrDefault(q => q.Id == entity.Id);
					Assert.IsNotNull(retrievedEntity);
					Assert.AreEqual(entity.Data, retrievedEntity.Data);
				}
			}
		}

		[TestMethod]
		public void DeleteEntities()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<SimplePoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<SimplePoco>();
			using (var dataProvider = new SQLite3DataProvider(TestHelper.ConnectionString))
			{
				dataProvider.ExecuteNonReader(CreateTableOperation.Create(model.EntityTable));

				var entities = new[]
				{
					new SimplePoco { Data = "Hello" },
					new SimplePoco { Data = "World" }
				};

				var database = new EntityDatabase<SimplePoco>(schema, dataProvider);

				database.Insert(entities);
				database.Delete(entities.Take(1));

				var queriedEntities = database.Query();
				Assert.AreEqual(1, queriedEntities.Count);
			}
		}

		[TestMethod]
		public void UpdateEntities()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<SimplePoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<SimplePoco>();
			using (var dataProvider = new SQLite3DataProvider(TestHelper.ConnectionString))
			{
				dataProvider.ExecuteNonReader(CreateTableOperation.Create(model.EntityTable));

				var entities = new[]
				{
					new SimplePoco { Data = "Hello" },
					new SimplePoco { Data = "World" }
				};

				var database = new EntityDatabase<SimplePoco>(schema, dataProvider);

				database.Insert(entities);

				var queriedEntities = database.Query().ToArray();
				queriedEntities[0].Data = "Hello Update";
				queriedEntities[1].Data = "World Update";
				database.Update(queriedEntities);

				var updatedQueriedEntities = database.Query();
				Assert.AreEqual(entities.Length, updatedQueriedEntities.Count);
				foreach (var entity in entities)
				{
					Assert.AreNotEqual(Guid.Empty, entity.Id);
					var retrievedEntity = updatedQueriedEntities.FirstOrDefault(q => q.Id == entity.Id);
					Assert.IsNotNull(retrievedEntity);
					Assert.AreEqual($"{entity.Data} Update", retrievedEntity.Data);
				}
			}
		}

		[TestMethod]
		public void QueryComputedValue()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PocoWithComputedValue>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<PocoWithComputedValue>();
			using (var dataProvider = new SQLite3DataProvider(TestHelper.ConnectionString))
			{
				dataProvider.ExecuteNonReader(CreateTableOperation.Create(model.EntityTable));

				var database = new EntityDatabase<PocoWithComputedValue>(schema, dataProvider);
				database.Insert(new[] { new PocoWithComputedValue { Value = "Hello" } });

				var entity = database.Query(
					where: database.Condition(q => q.ComputedValue == "hello").Build()
					).FirstOrDefault();
				Assert.IsNotNull(entity);
				Assert.AreEqual("hello", entity.ComputedValue);
			}
		}

		[TestMethod]
		public void QueryWithInlineMethod()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PocoWithComputedValue>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<PocoWithComputedValue>();
			using (var dataProvider = new SQLite3DataProvider(TestHelper.ConnectionString))
			{
				dataProvider.ExecuteNonReader(CreateTableOperation.Create(model.EntityTable));

				var database = new EntityDatabase<PocoWithComputedValue>(schema, dataProvider);
				database.Insert(new[] { new PocoWithComputedValue { Value = "Hello" } });

				var entity = database.Query(
					where: database.Condition(q => q.ComputedValue == "HeLLo".ToLowerInvariant()).Build()
					).FirstOrDefault();
				Assert.IsNotNull(entity);
				Assert.AreEqual("hello", entity.ComputedValue);
			}
		}

		private class SimplePoco
		{
			public Guid Id { get; private set; }
			public string Data { get; set; }
		}

		private class PocoWithComputedValue
		{
			public string Value { get; set; }
			public string ComputedValue => Value.ToLowerInvariant();
		}

		private enum SimpleEnum
		{
			One = 1,
			Two = 2,
			Three = 3
		}

		private class PocoWithEnum
		{
			public SimpleEnum SimpleEnum { get; set; }
		}
	}
}
