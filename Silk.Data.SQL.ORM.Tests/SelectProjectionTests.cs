using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class SelectProjectionTests
	{
		[TestMethod]
		public async Task ProjectPrimitives()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<SimplePocoModel>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await provider.ExecuteAsync(
					schema.CreateTable<SimplePocoModel>(),
					schema.CreateInsert(new SimplePocoModel { Data = 1 })
					);

				var selectQuery = schema.CreateSelect<SimplePocoModel, SimplePocoView>();
				await provider.ExecuteAsync(selectQuery);

				Assert.AreEqual(1, selectQuery.Result.Count);
				Assert.AreEqual(1, selectQuery.Result.First().Data);
			}
		}

		[TestMethod]
		public async Task ProjectFlattenedEmbeddedPrimitives()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<ParentPocoModel>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await provider.ExecuteAsync(
					schema.CreateTable<ParentPocoModel>(),
					schema.CreateInsert(new ParentPocoModel { Poco = new SimplePocoModel { Data = 1 } })
					);

				var selectQuery = schema.CreateSelect<ParentPocoModel, ParentPocoFlatView>();
				await provider.ExecuteAsync(selectQuery);

				Assert.AreEqual(1, selectQuery.Result.Count);
				Assert.AreEqual(1, selectQuery.Result.First().PocoData);
			}
		}

		[TestMethod]
		public async Task ProjectFlattenedRelatedPrimitives()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<ParentPocoModel>();
			schemaBuilder.DefineEntity<SimplePocoModel>();
			var schema = schemaBuilder.Build();

			var inObj = new ParentPocoModel { Poco = new SimplePocoModel { Data = 1 } };

			using (var provider = TestHelper.CreateProvider())
			{
				await provider.ExecuteAsync(
					schema.CreateTable<ParentPocoModel>(),
					schema.CreateTable<SimplePocoModel>(),
					schema.CreateInsert(inObj.Poco),
					schema.CreateInsert(inObj)
					);

				var selectQuery = schema.CreateSelect<ParentPocoModel, ParentPocoFlatView>();
				await provider.ExecuteAsync(selectQuery);

				Assert.AreEqual(1, selectQuery.Result.Count);
				Assert.AreEqual(1, selectQuery.Result.First().PocoData);
			}
		}

		[TestMethod]
		public async Task ProjectComplexEmbeddedData()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<ParentPocoModel>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await provider.ExecuteAsync(
					schema.CreateTable<ParentPocoModel>(),
					schema.CreateInsert(new ParentPocoModel { Poco = new SimplePocoModel { Data = 1 } })
					);

				var selectQuery = schema.CreateSelect<ParentPocoModel, ParentPocoComplexView>();
				await provider.ExecuteAsync(selectQuery);

				Assert.AreEqual(1, selectQuery.Result.Count);
				Assert.AreEqual(1, selectQuery.Result.First().Poco.Data);
			}
		}

		private class SimplePocoModel
		{
			public Guid Id { get; private set; }
			public int Data { get; set; }
		}

		private class SimplePocoView
		{
			public int Data { get; set; }
		}

		private class ParentPocoModel
		{
			public Guid Id { get; private set; }
			public SimplePocoModel Poco { get; set; }
		}

		private class ParentPocoFlatView
		{
			public Guid Id { get; private set; }
			public int PocoData { get; set; }
		}

		private class RelatedPoco
		{
			public Guid Id { get; private set; }
			public int Data { get; set; }
		}

		private class ParentPocoComplexView
		{
			public Guid Id { get; private set; }
			public SimplePocoView Poco { get; set; }
		}
	}
}
