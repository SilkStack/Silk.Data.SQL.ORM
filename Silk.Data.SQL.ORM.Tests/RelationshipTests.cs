using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class RelationshipTests
	{
		[TestMethod]
		public async Task CreateRelationshipTable()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<SimplePoco1>();
			schemaBuilder.DefineEntity<SimplePoco2>();
			schemaBuilder.DefineRelationship<SimplePoco1, SimplePoco2>("Relationship");
			var schema = schemaBuilder.Build();
			var tableName = schema.GetRelationship<SimplePoco1, SimplePoco2>("Relationship").JunctionTable.TableName;

			using (var provider = TestHelper.CreateProvider())
			{
				await provider.ExecuteAsync(
					schema.CreateTable<SimplePoco1>(),
					schema.CreateTable<SimplePoco2>(),
					schema.CreateTable<SimplePoco1, SimplePoco2>("Relationship")
					);

				await provider.ExecuteNonQueryAsync(SQLite3.SQLite3.Raw($"INSERT INTO {tableName} VALUES ('{Guid.NewGuid()}', '{Guid.NewGuid()}')"));
				using (var result = await provider.ExecuteReaderAsync(SQLite3.SQLite3.Raw($"SELECT * FROM {tableName}")))
				{
					Assert.IsTrue(result.HasRows);
				}
			}
		}

		private class SimplePoco1
		{
			public Guid Id { get; private set; }
			public int Data { get; set; }
		}

		private class SimplePoco2
		{
			public Guid Id { get; private set; }
			public int Data { get; set; }
		}
	}
}
