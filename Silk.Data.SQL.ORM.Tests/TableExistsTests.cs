//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Silk.Data.SQL.ORM.Schema;
//using System;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Silk.Data.SQL.ORM.Tests
//{
//	[TestClass]
//	public class TableExistsTests
//	{
//		[TestMethod]
//		public async Task TableExists()
//		{
//			var schemaBuilder = new SchemaBuilder();
//			schemaBuilder.DefineEntity<SimplePoco>();
//			var schema = schemaBuilder.Build();

//			using (var provider = TestHelper.CreateProvider())
//			{
//				await provider.ExecuteAsync(schema.CreateTable<SimplePoco>());

//				var tableExistsQuery = schema.CreateTableExists<SimplePoco>();
//				await provider.ExecuteAsync(tableExistsQuery);

//				Assert.IsTrue(tableExistsQuery.Result.First());
//			}
//		}

//		private class SimplePoco
//		{
//			public Guid Id { get; private set; }
//		}
//	}
//}
