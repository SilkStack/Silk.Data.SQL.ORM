using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Schema;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests.Schema
{
	[TestClass]
	public class SchemaBuilderTests
	{
		[TestMethod]
		public void Build_Returns_Populated_Schema()
		{
			var schema = new SchemaBuilder()
				.Define<ReferencedType>()
				.Define<EntityType>()
				.Build();
			Assert.IsNotNull(schema);
			Assert.AreEqual(1, schema.GetAll<EntityType>().Count());
			Assert.IsNotNull(schema.GetAll<EntityType>().First());
		}

		//  todo: move this test to an analyzer test set
		[TestMethod]
		public void CreateIntersection_Returns_Yes()
		{
			var entityModel = new SchemaBuilder()
				.Define<ReferencedType>()
				.Define<EntityType>()
				.Build()
				.GetAll<EntityType>()
				.First();
			var viewModel = TypeModel.GetModelOf<ViewType>();

			var analyzer = new EntityModelToTypeModelIntersectionAnalyzer();

			var intersection = analyzer.CreateIntersection(
				entityModel, viewModel
				);
		}

		private class ViewType
		{
			public int Id { get; private set; }
			public string Property { get; set; }

			public string EmbeddedProperty { get; set; }
			public int ReferencedId { get; set; }
			public string ReferencedProperty { get; set; }
		}

		private class EntityType
		{
			public int Id { get; private set; }
			public string Property { get; set; }

			public EmbeddedType Embedded { get; set; }
			public ReferencedType Referenced { get; set; }
		}

		private class EmbeddedType
		{
			public string Property { get; set; }
		}

		private class ReferencedType
		{
			public int Id { get; private set; }
			public string Property { get; set; }
		}
	}
}
