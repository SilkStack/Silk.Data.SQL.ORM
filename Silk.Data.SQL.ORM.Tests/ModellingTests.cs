using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class ModellingTests
	{
		[TestMethod]
		public void ModelSimplePoco()
		{
			var builder = new SchemaBuilder();
			builder.DefineEntity<SimplePoco>();
			var schema = builder.Build();
			var model = schema.GetEntityModel<SimplePoco>();

			Assert.IsNotNull(model);
			Assert.AreEqual(1, model.Fields.Length);
			Assert.IsNotNull(model.EntityTable);
		}

		private class SimplePoco
		{
			public int Value { get; set; }
		}
	}
}
