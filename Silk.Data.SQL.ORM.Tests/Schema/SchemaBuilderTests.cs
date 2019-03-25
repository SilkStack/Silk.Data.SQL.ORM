using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Schema;

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
			Assert.IsNotNull(schema.GetEntityModel<EntityType>());
		}

		[TestMethod]
		public void Build_Returns_Model_With_Index()
		{
			var schema = new SchemaBuilder()
				.Define<EntityType>(definition =>
				{
					definition.Index("idx", uniqueConstraint: true, indexFields: entity => entity.Property);
				})
				.Build();
			var entityModel = schema.GetEntityModel<EntityType>();

			Assert.AreEqual(1, entityModel.Indexes.Count);
			Assert.IsTrue(entityModel.Indexes[0].HasUniqueConstraint);
			Assert.AreEqual(1, entityModel.Indexes[0].Fields.Count);
			Assert.AreEqual("Property", entityModel.Indexes[0].Fields[0].FieldName);
		}

		//  todo: move this test to an analyzer test set
		[TestMethod]
		public void CreateIntersection_Returns_Yes()
		{
			var entityModel = new SchemaBuilder()
				.Define<ReferencedType>()
				.Define<EntityType>()
				.Build()
				.GetEntityModel<EntityType>();
			var viewModel = TypeModel.GetModelOf<ViewType>();

			var analyzer = new Data.Modelling.Analysis.DefaultIntersectionAnalyzer<EntityModel, EntityField, TypeModel, PropertyInfoField>();

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
