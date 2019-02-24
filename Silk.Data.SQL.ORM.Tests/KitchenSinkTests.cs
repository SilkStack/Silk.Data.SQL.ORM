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
					table.CreateTable(),
					store.Insert(entity),
					store.Select(entity.GetEntityReference(), out var entityResult)
				}.Execute();

				Assert.IsTrue(entityResult.TaskHasRun);
				Assert.IsFalse(entityResult.TaskFailed);
				var returnedEntity = entityResult.Result;

				Assert.AreNotSame(entity, returnedEntity);
				Assert.AreEqual(entity.Id, returnedEntity.Id);
			}
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

			public IEntityReference<FlatPoco> GetEntityReference()
				=> new Reference(this);

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
