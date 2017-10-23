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
			var domain = new DataDomain();
			var dataModel = domain.CreateDataModel<BasicPocoWithGuidId>();

			foreach (var table in dataModel.Tables)
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
		}

		[TestMethod]
		public void MultipleSelectGuidSimpleModel()
		{
			var domain = new DataDomain();
			var dataModel = domain.CreateDataModel<BasicPocoWithGuidId>();

			foreach (var table in dataModel.Tables)
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
				.Select(limit: 1, offset: 0)
				.Select(limit: 1, offset: 2)
				.Execute(TestDb.Provider);

			Assert.AreEqual(1, firstResults.Count);
			Assert.AreEqual(1, lastResults.Count);
			Assert.AreEqual(sourceInstances[0].Data, firstResults.First().Data);
			Assert.AreEqual(sourceInstances[2].Data, lastResults.First().Data);
		}

		private class BasicPocoWithGuidId
		{
			public Guid Id { get; private set; }
			public string Data { get; set; }
		}
	}
}
