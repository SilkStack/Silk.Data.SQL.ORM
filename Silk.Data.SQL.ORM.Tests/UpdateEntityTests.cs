using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Providers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class UpdateEntityTests
	{
		[TestMethod]
		public async Task UpdateFullEntityPrimitives()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PrimitivePoco>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				var obj = new PrimitivePoco { Data = 1 };
				await provider.ExecuteAsync(
					schema.CreateTable<PrimitivePoco>(),
					schema.CreateInsert(obj)
					);

				obj.Data = 2;
				var updateQuery = schema.CreateUpdate<PrimitivePoco>(obj);
				await provider.ExecuteAsync(updateQuery);

				var selectQuery = schema.CreateSelect<PrimitivePoco>();
				await provider.ExecuteAsync(selectQuery);

				Assert.AreEqual(1, selectQuery.Result.Count);
				Assert.AreEqual(obj.Data, selectQuery.Result.First().Data);
			}
		}

		[TestMethod]
		public async Task UpdateSingleFieldExpression()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PrimitivePoco>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				var obj = new PrimitivePoco { Data = 1 };
				await provider.ExecuteAsync(
					schema.CreateTable<PrimitivePoco>(),
					schema.CreateInsert(obj)
					);

				var queryBuilder = new EntityUpdateBuilder<PrimitivePoco>(schema);
				queryBuilder.Set(q => q.Data, q => q.Data + 1);
				await provider.ExecuteNonQueryAsync(queryBuilder.BuildQuery());

				var selectQuery = schema.CreateSelect<PrimitivePoco>();
				await provider.ExecuteAsync(selectQuery);

				Assert.AreEqual(1, selectQuery.Result.Count);
				Assert.AreEqual(2, selectQuery.Result.First().Data);
			}
		}

		private class PrimitivePoco
		{
			public Guid Id { get; private set; }
			public int Data { get; set; }
		}
	}
}
