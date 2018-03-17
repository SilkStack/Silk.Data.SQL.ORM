using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Operations;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class SelectTests
	{
		[TestMethod]
		public void SelectFlatPoco()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<FlatPoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<FlatPoco>();

			var select = SelectOperation.Create<FlatPoco>(model);

			throw new System.NotImplementedException();
		}

		private class FlatPoco
		{
			public int Id { get; set; }
			public string Data { get; set; }
		}
	}
}
