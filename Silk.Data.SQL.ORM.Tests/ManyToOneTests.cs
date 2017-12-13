using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Linq;
using System.Threading.Tasks;

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
			Assert.IsTrue(fieldForRelationshipA.Storage.IsNullable);

			Assert.IsNotNull(fieldForRelationshipB);
			Assert.IsNotNull(fieldForRelationshipB.Relationship);
			Assert.ReferenceEquals(model.Domain.GetEntityModel<RelationshipModelB>(), fieldForRelationshipB.Relationship.ForeignModel);
			Assert.AreEqual(RelationshipType.ManyToOne, fieldForRelationshipB.Relationship.RelationshipType);
			Assert.AreEqual("Id", fieldForRelationshipA.Relationship.ForeignField.Name);
			Assert.AreEqual(typeof(int), fieldForRelationshipB.Relationship.ForeignField.DataType);
			Assert.IsTrue(fieldForRelationshipB.Storage.IsNullable);

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
			Assert.IsTrue(fieldForRelationshipA.Storage.IsNullable);

			Assert.IsNotNull(fieldForRelationshipB);
			Assert.IsNotNull(fieldForRelationshipB.Relationship);
			Assert.ReferenceEquals(model.Domain.GetEntityModel<RelationshipModelB>(), fieldForRelationshipB.Relationship.ForeignModel);
			Assert.AreEqual(RelationshipType.ManyToOne, fieldForRelationshipB.Relationship.RelationshipType);
			Assert.AreEqual("Id", fieldForRelationshipA.Relationship.ForeignField.Name);
			Assert.AreEqual(typeof(int), fieldForRelationshipB.Relationship.ForeignField.DataType);
			Assert.IsTrue(fieldForRelationshipB.Storage.IsNullable);

			Assert.IsTrue(model.Schema.EntityTable.DataFields.Contains(fieldForRelationshipA));
			Assert.IsTrue(model.Schema.EntityTable.DataFields.Contains(fieldForRelationshipB));
		}

		[TestMethod]
		public async Task InsertWithNulls()
		{
			var model = _conventionDrivenModel;

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var objInstance = new ModelWithRelationships();
				await model.Domain.Insert(objInstance)
					.ExecuteAsync(TestDb.Provider);
			}
			finally
			{
				foreach (var entityModel in model.Domain.DataModels)
				{
					foreach (var table in entityModel.Schema.Tables)
					{
						await table.DropAsync(TestDb.Provider);
					}
				}
			}
		}

		[TestMethod]
		public async Task InsertFullyPopulated()
		{
			var model = _conventionDrivenModel;
			var relationshipAModel = _conventionDrivenModel.Domain.GetEntityModel<RelationshipModelA>();
			var relationshipBModel = _conventionDrivenModel.Domain.GetEntityModel<RelationshipModelB>();

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var objInstance = new ModelWithRelationships
				{
					RelationshipA = new RelationshipModelA { Data = 10 },
					RelationshipB = new RelationshipModelB { Data = 20 }
				};

				await relationshipAModel.Domain
					.Insert(objInstance.RelationshipA)
					.ExecuteAsync(TestDb.Provider);
				await relationshipBModel.Domain
					.Insert(objInstance.RelationshipB)
					.ExecuteAsync(TestDb.Provider);
				await model.Domain
					.Insert(objInstance)
					.ExecuteAsync(TestDb.Provider);
			}
			finally
			{
				foreach (var entityModel in model.Domain.DataModels)
				{
					foreach (var table in entityModel.Schema.Tables)
					{
						await table.DropAsync(TestDb.Provider);
					}
				}
			}
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
