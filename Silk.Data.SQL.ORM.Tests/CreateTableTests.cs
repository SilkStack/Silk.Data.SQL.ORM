﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class CreateTableTests
	{
		[TestMethod]
		public void DontCreateCompositePrimaryKeyWithSingleObjectRelationships()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<Poco>();
			schemaBuilder.DefineEntity<EmbeddedPoco>();
			var schema = schemaBuilder.Build();

			var relationshipColumn = schema.GetEntityModel<Poco>().EntityTable.Columns
				.First(q => q.ColumnName == nameof(Poco.Deep));

			Assert.IsFalse(relationshipColumn.IsPrimaryKey);
			Assert.IsFalse(relationshipColumn.IsClientGenerated);
			Assert.IsFalse(relationshipColumn.IsServerGenerated);
		}

		[TestMethod]
		public void IndexSingleRelationshipKey()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<Poco>()
				.For(x => x.Deep.Id).Index();
			schemaBuilder.DefineEntity<EmbeddedPoco>();
			var schema = schemaBuilder.Build();

			var relationshipColumn = schema.GetEntityModel<Poco>().EntityTable.Columns
				.First(q => q.ColumnName == nameof(Poco.Deep));
			Assert.IsNotNull(relationshipColumn.Index);
		}

		private class Poco
		{
			public Guid Id { get; private set; }
			public EmbeddedPoco Deep { get; set; }
		}

		private class EmbeddedPoco
		{
			public Guid Id { get; private set; }
		}
	}
}