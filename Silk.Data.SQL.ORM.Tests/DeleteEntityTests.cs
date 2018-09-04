using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Providers;
using System;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class DeleteEntityTests
	{
		[TestMethod]
		public async Task Delete()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PrimitivePoco>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await CreateSchema<PrimitivePoco>(schema, provider);

				var obj = new PrimitivePoco { Data = 1 };
				await provider.ExecuteAsync(schema.CreateInsert(obj));

				var deleteQuery = schema.CreateDelete<PrimitivePoco>(obj);
				await provider.ExecuteAsync(deleteQuery);

				var selectQuery = schema.CreateSelect<PrimitivePoco>();
				await provider.ExecuteAsync(selectQuery);
				Assert.AreEqual(0, selectQuery.Result.Count);
			}
		}

		private async Task CreateSchema<T>(Schema.Schema schema, IDataProvider dataProvider)
			where T : class
		{
			var createSchema = new EntityCreateSchemaBuilder<T>(schema);
			await dataProvider.ExecuteNonQueryAsync(createSchema.BuildQuery());
		}

		private class PrimitivePoco
		{
			public Guid Id { get; private set; }
			public int Data { get; set; }
		}
	}
}
