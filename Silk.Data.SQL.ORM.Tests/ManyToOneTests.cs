using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class ManyToOneTests
	{
		private static readonly EntityModel<ModelWithRelationships> _conventionDrivenModel =
			TestDb.CreateDomainAndModel<ModelWithRelationships>(builder =>
			{
				builder.AddDataEntity<RelationshipModelA>();
				builder.AddDataEntity<RelationshipModelB>();
			});
		private static readonly EntityModel<ModelWithRelationships, ModelWithRelationshipsView> _modelDrivenModel =
			TestDb.CreateDomainAndModel<ModelWithRelationships, ModelWithRelationshipsView>(builder =>
			{
				builder.AddDataEntity<RelationshipModelA>();
				builder.AddDataEntity<RelationshipModelB>();
			});

		[TestMethod]
		public void ConventionDrivenModelManyToOneRelationship()
		{
			var model = _conventionDrivenModel;

			Assert.AreEqual(3, model.Fields.Length);

			var fieldForRelationshipA = model.Fields.FirstOrDefault(q => q.Name == "RelationshipAId");
			var fieldForRelationshipB = model.Fields.FirstOrDefault(q => q.Name == "RelationshipBId");

			Assert.IsNotNull(fieldForRelationshipA);
			Assert.IsNotNull(fieldForRelationshipA.Relationship);
			Assert.ReferenceEquals(model.Domain.GetEntityModel<RelationshipModelA>(), fieldForRelationshipA.Relationship.ForeignModel);
			Assert.AreEqual(RelationshipType.ManyToOne, fieldForRelationshipA.Relationship.RelationshipType);
			Assert.AreEqual("Id", fieldForRelationshipA.Relationship.ForeignField.Name);
			Assert.AreEqual(typeof(Guid), fieldForRelationshipA.Relationship.ForeignField.DataType);

			Assert.IsNotNull(fieldForRelationshipB);
			Assert.IsNotNull(fieldForRelationshipB.Relationship);
			Assert.ReferenceEquals(model.Domain.GetEntityModel<RelationshipModelB>(), fieldForRelationshipB.Relationship.ForeignModel);
			Assert.AreEqual(RelationshipType.ManyToOne, fieldForRelationshipB.Relationship.RelationshipType);
			Assert.AreEqual("Id", fieldForRelationshipA.Relationship.ForeignField.Name);
			Assert.AreEqual(typeof(int), fieldForRelationshipB.Relationship.ForeignField.DataType);

			Assert.IsTrue(model.Schema.EntityTable.DataFields.Contains(fieldForRelationshipA));
			Assert.IsTrue(model.Schema.EntityTable.DataFields.Contains(fieldForRelationshipB));
		}

		[TestMethod]
		public void ModelDrivenModelManyToOneRelationship()
		{
			var model = _modelDrivenModel;

			Assert.AreEqual(3, model.Fields.Length);

			var fieldForRelationshipA = model.Fields.FirstOrDefault(q => q.Name == "RelationshipAId");
			var fieldForRelationshipB = model.Fields.FirstOrDefault(q => q.Name == "RelationshipBId");

			Assert.IsNotNull(fieldForRelationshipA);
			Assert.IsNotNull(fieldForRelationshipA.Relationship);
			Assert.ReferenceEquals(model.Domain.GetEntityModel<RelationshipModelA>(), fieldForRelationshipA.Relationship.ForeignModel);
			Assert.AreEqual(RelationshipType.ManyToOne, fieldForRelationshipA.Relationship.RelationshipType);
			Assert.AreEqual("Id", fieldForRelationshipA.Relationship.ForeignField.Name);
			Assert.AreEqual(typeof(Guid), fieldForRelationshipA.Relationship.ForeignField.DataType);

			Assert.IsNotNull(fieldForRelationshipB);
			Assert.IsNotNull(fieldForRelationshipB.Relationship);
			Assert.ReferenceEquals(model.Domain.GetEntityModel<RelationshipModelB>(), fieldForRelationshipB.Relationship.ForeignModel);
			Assert.AreEqual(RelationshipType.ManyToOne, fieldForRelationshipB.Relationship.RelationshipType);
			Assert.AreEqual("Id", fieldForRelationshipA.Relationship.ForeignField.Name);
			Assert.AreEqual(typeof(int), fieldForRelationshipB.Relationship.ForeignField.DataType);

			Assert.IsTrue(model.Schema.EntityTable.DataFields.Contains(fieldForRelationshipA));
			Assert.IsTrue(model.Schema.EntityTable.DataFields.Contains(fieldForRelationshipB));
		}

		private class ModelWithRelationships
		{
			public Guid Id { get; private set; }
			public RelationshipModelA RelationshipA { get; set; }
			public RelationshipModelB RelationshipB { get; set; }
		}

		private class ModelWithRelationshipsView
		{
			public Guid Id { get; private set; }
			public Guid RelationshipAId { get; set; }
			public int RelationshipBId { get; set; }
		}

		private class RelationshipModelA
		{
			public Guid Id { get; private set; }
			public int Data { get; set; }
		}

		private class RelationshipModelB
		{
			public int Id { get; private set; }
			public int Data { get; set; }
		}
	}
}
