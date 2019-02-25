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
					store.Insert(entity),
					//  retrieve the entity previously inserted
					store.Select(entity.GetEntityReference(), out var entityResult),
					//  update the entities `String` value
					store.Update(entity.GetEntityReference(), new { String = "New String" }),
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

		private class StringView
		{
			public string String { get; private set; }
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
