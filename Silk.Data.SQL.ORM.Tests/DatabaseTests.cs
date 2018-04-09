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
		public void InsertAndSelect()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<SimplePoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<SimplePoco>();
			using (var dataProvider = new SQLite3DataProvider(":memory:"))
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
						.Condition(q => q.Id == entities[0].Id)
						.Or(q => q.Id == entities[1].Id)
						.Build()
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

		private class SimplePoco
		{
			public Guid Id { get; private set; }
			public string Data { get; set; }
		}
	}
}
