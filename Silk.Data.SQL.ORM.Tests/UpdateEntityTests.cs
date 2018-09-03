﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Providers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class UpdateEntityTests
	{
		[TestMethod]
		public async Task UpdatePrimitives()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<PrimitivePoco>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await CreateSchema<PrimitivePoco>(schema, provider);

				var obj = new PrimitivePoco { Data = 1 };
				var queryBuilder = new EntityInsertBuilder<PrimitivePoco>(schema);
				queryBuilder.Add(obj);
				await provider.ExecuteNonQueryAsync(queryBuilder.BuildQuery());

				obj.Data = 2;
				var updateQuery = schema.CreateUpdateQuery<PrimitivePoco>(obj);
				await provider.ExecuteNonQueryAsync(updateQuery);

				var selectBuilder = new EntitySelectBuilder<PrimitivePoco>(schema);
				var mapping = selectBuilder.Project(q => q.Data);
				using (var queryResult = await provider.ExecuteReaderAsync(selectBuilder.BuildQuery()))
				{
					Assert.IsTrue(await queryResult.ReadAsync());
					Assert.AreEqual(obj.Data, mapping.Read(queryResult));
				}
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
