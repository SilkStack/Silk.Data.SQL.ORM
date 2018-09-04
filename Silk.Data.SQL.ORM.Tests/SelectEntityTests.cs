using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class SelectEntityTests
	{
		[TestMethod]
		public async Task Count()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PrimitivePoco>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await CreateSchema<PrimitivePoco>(schema, provider);

				var inObjs = new[]
				{
					new PrimitivePoco { Data = 2 },
					new PrimitivePoco { Data = 3 },
					new PrimitivePoco { Data = 1 }
				};
				await provider.ExecuteAsync(schema.CreateInsert(inObjs));

				var selectQuery = schema.CreateCount<PrimitivePoco>(q => q.AndWhere(obj => obj.Data == 1 || obj.Data == 3));
				await provider.ExecuteAsync(selectQuery);
				Assert.AreEqual(1, selectQuery.Result.Count);
				Assert.AreEqual(2, selectQuery.Result.First());
			}
		}

		[TestMethod]
		public async Task Where()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PrimitivePoco>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await CreateSchema<PrimitivePoco>(schema, provider);

				var inObjs = new[]
				{
					new PrimitivePoco { Data = 2 },
					new PrimitivePoco { Data = 3 },
					new PrimitivePoco { Data = 1 }
				};
				await provider.ExecuteAsync(schema.CreateInsert(inObjs));

				var selectQuery = schema.CreateSelect<PrimitivePoco>(q => q.AndWhere(obj => obj.Data == 1 || obj.Data == 3));
				await provider.ExecuteAsync(selectQuery);
				Assert.AreEqual(2, selectQuery.Result.Count);
			}
		}

		[TestMethod]
		public async Task Having()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PrimitivePoco>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await CreateSchema<PrimitivePoco>(schema, provider);

				var inObjs = new[]
				{
					new PrimitivePoco { Data = 2 },
					new PrimitivePoco { Data = 3 },
					new PrimitivePoco { Data = 1 }
				};
				await provider.ExecuteAsync(schema.CreateInsert(inObjs));

				var selectQuery = schema.CreateSelect<PrimitivePoco>(q =>
				{
					q.AndHaving(obj => obj.Data == 1 || obj.Data == 3);
					q.GroupBy(obj => obj.Id);
				});
				await provider.ExecuteAsync(selectQuery);
				Assert.AreEqual(2, selectQuery.Result.Count);
			}
		}

		[TestMethod]
		public async Task GroupBy()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PrimitivePoco>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await CreateSchema<PrimitivePoco>(schema, provider);

				var inObjs = new[]
				{
					new PrimitivePoco { Data = 1 },
					new PrimitivePoco { Data = 1 },
					new PrimitivePoco { Data = 1 }
				};
				await provider.ExecuteAsync(schema.CreateInsert(inObjs));

				var selectQuery = schema.CreateSelect<PrimitivePoco>(q => q.GroupBy(obj => obj.Data));
				await provider.ExecuteAsync(selectQuery);
				Assert.AreEqual(1, selectQuery.Result.Count);
			}
		}

		[TestMethod]
		public async Task OrderBy()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PrimitivePoco>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await CreateSchema<PrimitivePoco>(schema, provider);

				var inObjs = new[]
				{
					new PrimitivePoco { Data = 2 },
					new PrimitivePoco { Data = 3 },
					new PrimitivePoco { Data = 1 }
				};
				await provider.ExecuteAsync(schema.CreateInsert(inObjs));

				var selectAscending = schema.CreateSelect<PrimitivePoco>(q => q.OrderBy(obj => obj.Data));
				var selectDescending = schema.CreateSelect<PrimitivePoco>(q => q.OrderBy(obj => obj.Data, OrderDirection.Descending));
				await provider.ExecuteAsync(selectAscending, selectDescending);

				Assert.IsTrue(
					selectAscending.Result.Select(q => q.Data).SequenceEqual(new[] { 1, 2, 3 })
					);
				Assert.IsTrue(
					selectDescending.Result.Select(q => q.Data).SequenceEqual(new[] { 3, 2, 1 })
					);
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
