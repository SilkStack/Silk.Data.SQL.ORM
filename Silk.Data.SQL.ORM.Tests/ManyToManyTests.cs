using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections.Generic;

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

			Assert.Fail("Test not complete");
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
