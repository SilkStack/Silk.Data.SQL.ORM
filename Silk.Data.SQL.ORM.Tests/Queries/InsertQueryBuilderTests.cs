using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Tests.Queries
{
	[TestClass]
	public class InsertQueryBuilderTests
	{
		[TestMethod]
		public void Create_From_Entity_Returns_Entity_Insert()
		{
			var schema = new SchemaBuilder()
				.Define<SimpleEntity>()
				.Build();
			var entity = new SimpleEntity { Property = "1234" };
			var query = InsertBuilder<SimpleEntity>.Create(schema, entity);
		}

		[TestMethod]
		public void Create_From_View_Returns_Entity_Insert()
		{
			var schema = new SchemaBuilder()
				.Define<SimpleEntity>()
				.Build();
			var entity = new { Property = "1234" };
			var query = InsertBuilder<SimpleEntity>.Create(schema, entity);
		}

		private class SimpleEntity
		{
			public string Property { get; set; }
		}
	}
}
