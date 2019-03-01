using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.SQLite3;
using System;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class KitchenSinkTests
	{
		[TestMethod]
		public void Single_Flat_POCO()
		{
			using (var dataProvider = new SQLite3DataProvider("Data Source=:memory:;Mode=Memory"))
			{
				var schema = new SchemaBuilder()
					.Define<FlatPoco>()
					.Build();

				var table = new EntityTable<FlatPoco>(schema, dataProvider);
				var store = new SqlEntityStore<FlatPoco>(schema, dataProvider);

				var entity = new FlatPoco
				{
					String = "Test String"
				};

				new[]
				{
					//  create table
					table.CreateTable(),
					//  insert a single entity
					store.Insert(entity).Defer(),
					//  retrieve the entity previously inserted
					store.Select(entity.GetEntityReference(), out var entityResult),
					//  update the entities `String` value
					store.Update(entity.GetEntityReference(), new { String = "New String" }).Defer(),
					//  select the entity again in the shape of `StringView`
					store.Select<StringView>(entity.GetEntityReference(), out var stringViewResult),
					//  select every ID from the table
					store.Select(q => q.Id, query => { }, out var allIdsResult),
					//  count the entities
					store.Select(q => DatabaseFunctions.Count(q), query => { }, out var countResult),
					//  load all entities
					store.Select(query => { }, out var allEntitiesResult),
					//  load all entities in the shape of `StringView`
					store.Select<StringView>(query => { }, out var allStringViewsResult)
				}.Execute();

				Assert.IsTrue(entityResult.TaskHasRun);
				Assert.IsFalse(entityResult.TaskFailed);
				var returnedEntity = entityResult.Result;

				Assert.AreNotSame(entity, returnedEntity);
				Assert.AreEqual(entity.Id, returnedEntity.Id);

				Assert.IsTrue(stringViewResult.TaskHasRun);
				Assert.IsFalse(stringViewResult.TaskFailed);
				Assert.AreEqual("New String", stringViewResult.Result.String);

				Assert.IsTrue(allIdsResult.TaskHasRun);
				Assert.IsFalse(allIdsResult.TaskFailed);
				Assert.AreEqual(1, allIdsResult.Result.Count);
				Assert.AreEqual(entity.Id, allIdsResult.Result[0]);

				Assert.IsTrue(countResult.TaskHasRun);
				Assert.IsFalse(countResult.TaskFailed);
				Assert.AreEqual(1, countResult.Result.Count);
				Assert.AreEqual(1, countResult.Result[0]);

				Assert.IsTrue(allEntitiesResult.TaskHasRun);
				Assert.IsFalse(allEntitiesResult.TaskFailed);
				Assert.AreEqual(1, allEntitiesResult.Result.Count);
				Assert.AreEqual(entity.Id, allEntitiesResult.Result[0].Id);

				Assert.IsTrue(allStringViewsResult.TaskHasRun);
				Assert.IsFalse(allStringViewsResult.TaskFailed);
				Assert.AreEqual(1, allStringViewsResult.Result.Count);
				Assert.AreEqual("New String", allStringViewsResult.Result[0].String);
			}
		}

		[TestMethod]
		public void Single_Embedded_POCO()
		{
			using (var dataProvider = new SQLite3DataProvider("Data Source=:memory:;Mode=Memory"))
			{
				var schema = new SchemaBuilder()
					.Define<ParentPoco>()
					.Build();

				var table = new EntityTable<ParentPoco>(schema, dataProvider);
				var store = new SqlEntityStore<ParentPoco>(schema, dataProvider);

				var entity = new ParentPoco
				{
					Flat = new FlatPoco
					{
						String = "Some String"
					}
				};

				new[]
				{
					table.CreateTable(),
					store.Insert(entity).Defer(),
					store.Select(entity.GetEntityReference(), out var retreivedEntityResult),
					store.Select<EmbeddedStringView>(entity.GetEntityReference(), out var stringViewResult)
				}.Execute();
			}
		}

		[TestMethod]
		public void Single_Related_POCO()
		{
			using (var dataProvider = new SQLite3DataProvider("Data Source=:memory:;Mode=Memory"))
			{
				var schema = new SchemaBuilder()
					.Define<FlatPoco>()
					.Define<ParentPoco>()
					.Build();

				var parentTable = new EntityTable<ParentPoco>(schema, dataProvider);
				var parentStore = new SqlEntityStore<ParentPoco>(schema, dataProvider);
				var childTable = new EntityTable<FlatPoco>(schema, dataProvider);
				var childStore = new SqlEntityStore<FlatPoco>(schema, dataProvider);

				var entity = new ParentPoco
				{
					Flat = new FlatPoco
					{
						String = "Some String"
					}
				};

				var transaction = new Transaction();
				transaction.Execute(
					new[]
					{
						parentTable.CreateTable(),
						childTable.CreateTable(),

						childStore.Insert(entity.Flat).Defer(),
						parentStore.Insert(entity).Defer(),

						parentStore.Select(entity.GetEntityReference(), out var retreivedEntityResult),
						parentStore.Select<EmbeddedStringView>(entity.GetEntityReference(), out var stringViewResult)
					});
				transaction.Execute(new[]{
					parentStore.Select<EmbeddedStringView>(query => query.AndWhere(q => q.Flat.Id == entity.Flat.Id).Limit(1), out var customQueryResult)
				});
				transaction.Rollback();
			}
		}

		private class ParentPoco
		{
			public Guid Id { get; private set; }
			public FlatPoco Flat { get; set; }

			public IEntityReference<ParentPoco> GetEntityReference()
				=> new Reference(this);

			public enum PocoEnum
			{
				ValueOne,
				ValueTwo
			}

			private class Reference : IEntityReference<ParentPoco>
			{
				private readonly ParentPoco _referenceEntity;

				public Reference(ParentPoco referenceEntity)
				{
					_referenceEntity = referenceEntity;
				}

				public ParentPoco AsEntity()
					=> _referenceEntity;
			}
		}

		private class EmbeddedStringView
		{
			public string FlatString { get; set; }
		}

		private class StringView
		{
			public string String { get; private set; }
			public string ComputedValue { get; private set; }
		}

		private class FlatPoco
		{
			public Guid Id { get; private set; }

			public bool Bool { get; set; }
			public byte Byte { get; set; }
			public short Short { get; set; }
			public int Int { get; set; }
			public long Long { get; set; }
			public float Float { get; set; }
			public double Double { get; set; }
			public decimal Decimal { get; set; }
			public string String { get; set; }
			public DateTime DateTime { get; set; }
			public Guid Guid { get; set; }
			public PocoEnum Enum { get; set; }

			public string ComputedValue => "Hello World";

			public IEntityReference<FlatPoco> GetEntityReference()
				=> new Reference(this);

			public enum PocoEnum
			{
				ValueOne,
				ValueTwo
			}

			private class Reference : IEntityReference<FlatPoco>
			{
				private readonly FlatPoco _referenceEntity;

				public Reference(FlatPoco referenceEntity)
				{
					_referenceEntity = referenceEntity;
				}

				public FlatPoco AsEntity()
					=> _referenceEntity;
			}
		}
	}
}
