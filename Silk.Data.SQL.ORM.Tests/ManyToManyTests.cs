using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class ManyToManyTests
	{
		private EntityModel<PocoWithManyRelationships> _conventionDrivenModel =
			TestDb.CreateDomainAndModel<PocoWithManyRelationships>(builder => {
				builder.AddDataEntity<RelationshipTypeA>();
				builder.AddDataEntity<RelationshipTypeB>();
			});

		[TestMethod]
		public void ConventionDrivenModelModelsManyToMany()
		{
			var model = _conventionDrivenModel;
			Assert.AreEqual(3, model.Fields.Length);
			Assert.AreEqual(3, model.Schema.Tables.Count);

			var fieldForRelationshipA = model.Fields.FirstOrDefault(q => q.Name == "RelationshipA");
			var fieldForRelationshipB = model.Fields.FirstOrDefault(q => q.Name == "RelationshipB");

			Assert.IsNotNull(fieldForRelationshipA);
			Assert.IsNotNull(fieldForRelationshipA.Relationship);
			Assert.ReferenceEquals(model.Domain.GetEntityModel<RelationshipTypeA>(), fieldForRelationshipA.Relationship.ForeignModel);
			Assert.AreEqual(RelationshipType.ManyToMany, fieldForRelationshipA.Relationship.RelationshipType);
			Assert.IsNull(fieldForRelationshipA.Storage);

			Assert.IsNotNull(fieldForRelationshipB);
			Assert.IsNotNull(fieldForRelationshipB.Relationship);
			Assert.ReferenceEquals(model.Domain.GetEntityModel<RelationshipTypeB>(), fieldForRelationshipB.Relationship.ForeignModel);
			Assert.AreEqual(RelationshipType.ManyToMany, fieldForRelationshipB.Relationship.RelationshipType);
			Assert.IsNull(fieldForRelationshipB.Storage);

			Assert.AreEqual(1, model.Schema.EntityTable.DataFields.Length);
			var idField = model.Schema.EntityTable.DataFields.FirstOrDefault(q => q.Name == "Id");
			Assert.IsNotNull(idField);
			Assert.AreEqual(typeof(Guid), idField.DataType);
			Assert.IsTrue(idField.Storage.IsAutoGenerate);
			Assert.IsTrue(idField.Storage.IsPrimaryKey);

			var relationshipATable = model.Schema.RelationshipTables.FirstOrDefault(q => q.TableName == "PocoWithManyRelationshipsToRelationshipTypeA");
			Assert.AreEqual(2, relationshipATable.DataFields.Length);
			var modelIdField = relationshipATable.DataFields.FirstOrDefault(q => q.Name == "PocoWithManyRelationships_Id");
			Assert.IsNotNull(modelIdField);
			Assert.AreEqual(typeof(Guid), modelIdField.DataType);
			var relationshipIdField = relationshipATable.DataFields.FirstOrDefault(q => q.Name == "RelationshipTypeA_Id");
			Assert.IsNotNull(relationshipIdField);
			Assert.AreEqual(typeof(int), relationshipIdField.DataType);

			var relationshipBTable = model.Schema.RelationshipTables.FirstOrDefault(q => q.TableName == "PocoWithManyRelationshipsToRelationshipTypeB");
			Assert.AreEqual(2, relationshipBTable.DataFields.Length);
			modelIdField = relationshipBTable.DataFields.FirstOrDefault(q => q.Name == "PocoWithManyRelationships_Id");
			Assert.IsNotNull(modelIdField);
			Assert.AreEqual(typeof(Guid), modelIdField.DataType);
			relationshipIdField = relationshipBTable.DataFields.FirstOrDefault(q => q.Name == "RelationshipTypeB_Id");
			Assert.IsNotNull(relationshipIdField);
			Assert.AreEqual(typeof(Guid), relationshipIdField.DataType);
		}

		private class PocoWithManyRelationships
		{
			public Guid Id { get; private set; }
			public List<RelationshipTypeA> RelationshipA { get; set; } = new List<RelationshipTypeA>();
			public RelationshipTypeB[] RelationshipB { get; set; } = new RelationshipTypeB[0];
		}

		private class RelationshipTypeA
		{
			public int Id { get; private set; }
			public int Data { get; set; }
		}

		private class RelationshipTypeB
		{
			public Guid Id { get; private set; }
			public int Data { get; set; }
		}
	}
}
