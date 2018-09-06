using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
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
			var relationship = schema.GetRelationship<SimplePoco1, SimplePoco2>("Relationship");
			var tableName = relationship.JunctionTable.TableName;
			var columnNames = relationship.RelationshipFields
				.SelectMany(q => q.Columns).Select(q => q.ColumnName).ToArray();

			using (var provider = TestHelper.CreateProvider())
			{
				await provider.ExecuteAsync(
					schema.CreateTable<SimplePoco1>(),
					schema.CreateTable<SimplePoco2>(),
					schema.CreateTable<SimplePoco1, SimplePoco2>("Relationship")
					);

				await provider.ExecuteNonQueryAsync(QueryExpression.Insert(tableName, columnNames, new object[] { Guid.NewGuid(), Guid.NewGuid() }));
				using (var result = await provider.ExecuteReaderAsync(QueryExpression.Select(QueryExpression.All(), QueryExpression.Table(tableName))))
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
