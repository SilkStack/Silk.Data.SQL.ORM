using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Schema;
using System;

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

			throw new NotImplementedException();
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

			throw new NotImplementedException();
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

			throw new NotImplementedException();
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
