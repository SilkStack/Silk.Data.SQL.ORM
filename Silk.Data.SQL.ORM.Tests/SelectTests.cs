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
			dataModel.Domain.Insert(sourceInstances)
				.Execute(TestDb.Provider);

			var queriedInstances = dataModel.Domain.Select<BasicPocoWithGuidId>()
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
