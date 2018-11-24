using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class MultipleEntityDefinitionTests
	{
		[TestMethod]
		public void CanDefineSameEntityTwice()
		{
			var definitionOne = new EntitySchemaDefinition<Entity>
			{
				TableName = "Table1"
			};

			var definitionTwo = new EntitySchemaDefinition<Entity>
			{
				TableName = "Table2"
			};

			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.AddDefinition(definitionOne);
			schemaBuilder.AddDefinition(definitionTwo);

			var schema = schemaBuilder.Build();

			var entitySchemaOne = schema.GetEntitySchema(definitionOne);
			var entitySchemaTwo = schema.GetEntitySchema(definitionTwo);

			Assert.IsNotNull(entitySchemaOne);
			Assert.IsNotNull(entitySchemaTwo);
			Assert.IsFalse(ReferenceEquals(entitySchemaOne, entitySchemaTwo));
		}

		[TestMethod]
		public async Task EntitiesAreStoredSeparately()
		{
			var definitionOne = new EntitySchemaDefinition<Entity>
			{
				TableName = "Table1"
			};

			var definitionTwo = new EntitySchemaDefinition<Entity>
			{
				TableName = "Table2"
			};

			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.AddDefinition(definitionOne);
			schemaBuilder.AddDefinition(definitionTwo);

			var schema = schemaBuilder.Build();

			var entitySchemaOne = schema.GetEntitySchema(definitionOne);
			var entitySchemaTwo = schema.GetEntitySchema(definitionTwo);

			using (var dbProvider = TestHelper.CreateProvider())
			{
				await dbProvider.ExecuteAsync(
					entitySchemaOne.CreateTable(),
					entitySchemaTwo.CreateTable()
					);

				var inObj1 = new Entity { Data = 1 };
				var inObj2 = new Entity { Data = 2 };

				await dbProvider.ExecuteAsync(
					entitySchemaOne.CreateInsert(inObj1),
					entitySchemaTwo.CreateInsert(inObj2)
					);

				var outObjs1 = default(ICollection<Entity>);
				var outObjs2 = default(ICollection<Entity>);

				await dbProvider.ExecuteAsync(
					entitySchemaOne.CreateSelect()
						.WithResult(r => outObjs1 = r),
					entitySchemaTwo.CreateSelect()
						.WithResult(r => outObjs2 = r)
					);

				Assert.AreEqual(1, outObjs1.Count);
				Assert.AreEqual(1, outObjs2.Count);
				Assert.AreEqual(inObj1.Data, outObjs1.First().Data);
				Assert.AreEqual(inObj2.Data, outObjs2.First().Data);
			}
		}

		private class Entity
		{
			public int Data { get; set; }
		}
	}
}
