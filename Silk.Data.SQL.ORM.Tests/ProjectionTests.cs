using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class ProjectionTests
	{
		[TestMethod]
		public void ProjectCopySameType()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<Entity>();
			schemaBuilder.DefineEntity<RelatedPoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<Entity>();
			var projection = model.GetProjection<DataProjection>();

			Assert.IsNotNull(projection);
			Assert.AreEqual(1, projection.Fields.Length);
			var field = projection.Fields.OfType<IValueField>().FirstOrDefault();
			Assert.IsNotNull(field);
			Assert.AreEqual("Data", field.FieldName);
		}

		[TestMethod]
		public void ProjectFlattenedRelatedSameType()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<Entity>();
			schemaBuilder.DefineEntity<RelatedPoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<Entity>();
			var projection = model.GetProjection<RelatedDataProjection>();

			Assert.IsNotNull(projection);
			Assert.AreEqual(1, projection.Fields.Length);
			var field = projection.Fields.OfType<ISingleRelatedObjectField>().FirstOrDefault();
			Assert.IsNotNull(field);
			Assert.AreEqual("Related", field.FieldName);
			Assert.AreEqual(1, field.RelatedObjectProjection.Fields.Length);
			var valueField = field.RelatedObjectProjection.Fields.OfType<IValueField>().FirstOrDefault();
			Assert.IsNotNull(valueField);
			Assert.AreEqual("Data", valueField.FieldName);
		}

		[TestMethod]
		public void ProjectFlattenedEmbeddedSameType()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<Entity>();
			schemaBuilder.DefineEntity<RelatedPoco>();
			var schema = schemaBuilder.Build();
			var model = schema.GetEntityModel<Entity>();
			var projection = model.GetProjection<EmbeddedStrProjection>();

			Assert.IsNotNull(projection);
			Assert.AreEqual(1, projection.Fields.Length);
			var field = projection.Fields.OfType<IEmbeddedObjectField>().FirstOrDefault();
			Assert.IsNotNull(field);
			Assert.AreEqual("Embedded", field.FieldName);
			Assert.AreEqual(1, field.EmbeddedFields.Length);
			var valueField = field.EmbeddedFields.OfType<IValueField>().FirstOrDefault();
			Assert.IsNotNull(valueField);
			Assert.AreEqual("Str", valueField.FieldName);
		}

		private class Entity
		{
			public Guid Id { get; set; }
			public string Data { get; set; }
			public RelatedPoco Related { get; set; }
			public EmbeddedPoco Embedded { get; set; }
		}

		private class EmbeddedPoco
		{
			public int Int { get; set; }
			public string Str { get; set; }
		}

		private class RelatedPoco
		{
			public int Id { get; set; }
			public string Data { get; set; }
		}

		private class DataProjection
		{
			public string Data { get; set; }
		}

		private class RelatedDataProjection
		{
			public string RelatedData { get; set; }
		}

		private class EmbeddedStrProjection
		{
			public string EmbeddedStr { get; set; }
		}
	}
}
