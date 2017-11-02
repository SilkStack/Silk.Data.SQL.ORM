using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class SelectTests
	{
		[TestMethod]
		public void SelectGuidSimpleModel()
		{
			var dataModel = TestDb.CreateDomainAndModel<BasicPocoWithGuidId>();

			foreach (var table in dataModel.Schema.Tables)
			{
				if (!table.Exists(TestDb.Provider))
					table.Create(TestDb.Provider);
			}

			var sourceInstances = new[]
			{
				new BasicPocoWithGuidId { Data = "Hello World 1" },
				new BasicPocoWithGuidId { Data = "Hello World 2" },
				new BasicPocoWithGuidId { Data = "Hello World 3" }
			};
			dataModel.Insert(sourceInstances)
				.Execute(TestDb.Provider);

			var queriedInstances = dataModel.Select()
				.Execute(TestDb.Provider);
			Assert.AreEqual(sourceInstances.Length, queriedInstances.Count);
			foreach (var sourceInstance in sourceInstances)
			{
				Assert.IsTrue(queriedInstances.Any(q => q.Id == sourceInstance.Id && q.Data == sourceInstance.Data));
			}

			foreach (var table in dataModel.Schema.Tables)
			{
				table.Drop(TestDb.Provider);
			}
		}

		[TestMethod]
		public void MultipleSelectGuidSimpleModel()
		{
			var dataModel = TestDb.CreateDomainAndModel<BasicPocoWithGuidId, BasicPocoWithGuidId>();

			foreach (var table in dataModel.Schema.Tables)
			{
				if (!table.Exists(TestDb.Provider))
					table.Create(TestDb.Provider);
			}

			var sourceInstances = new[]
			{
				new BasicPocoWithGuidId { Data = "Hello World 1" },
				new BasicPocoWithGuidId { Data = "Hello World 2" },
				new BasicPocoWithGuidId { Data = "Hello World 3" }
			};

			var (firstResults, lastResults) = dataModel
				.Insert(sourceInstances)
				.Select(where: dataModel.Where(q => q.Id == sourceInstances[0].Id))
				.Select(where: dataModel.Where(q => q.Id == sourceInstances[2].Id))
				.AsTransaction()
				.Execute(TestDb.Provider);

			Assert.AreEqual(1, firstResults.Count);
			Assert.AreEqual(1, lastResults.Count);
			Assert.AreEqual(sourceInstances[0].Data, firstResults.First().Data);
			Assert.AreEqual(sourceInstances[2].Data, lastResults.First().Data);

			foreach (var table in dataModel.Schema.Tables)
			{
				table.Drop(TestDb.Provider);
			}
		}

		[TestMethod]
		public void SimpleViewProjection()
		{
			var dataModel = TestDb.CreateDomainAndModel<BasicPocoWithGuidId, BasicPocoWithGuidId>();

			foreach (var table in dataModel.Schema.Tables)
			{
				if (!table.Exists(TestDb.Provider))
					table.Create(TestDb.Provider);
			}

			var sourceInstances = new[]
			{
				new BasicPocoWithGuidId { Data = "Hello World 1" },
				new BasicPocoWithGuidId { Data = "Hello World 2" },
				new BasicPocoWithGuidId { Data = "Hello World 3" }
			};

			var dataResults = dataModel
				.Insert(sourceInstances)
				.Select<BasicPocoView>()
				.AsTransaction()
				.Execute(TestDb.Provider);

			Assert.AreEqual(sourceInstances.Length, dataResults.Count);
			foreach (var sourceInstance in sourceInstances)
			{
				Assert.IsTrue(dataResults.Any(q => q.Data == sourceInstance.Data));
			}

			foreach (var table in dataModel.Schema.Tables)
			{
				table.Drop(TestDb.Provider);
			}
		}

		private class BasicPocoWithGuidId
		{
			public Guid Id { get; private set; }
			public string Data { get; set; }
		}

		private class BasicPocoView
		{
			public string Data { get; set; }
		}
	}
}
