﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class SelectTests
	{
		[TestMethod]
		public void InsertGuidSimpleModel()
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
			dataModel.Insert(TestDb.Provider, sourceInstances);

			var queriedInstances = dataModel.Select(TestDb.Provider);
			Assert.AreEqual(sourceInstances.Length, queriedInstances.Count);
			foreach (var sourceInstance in sourceInstances)
			{
				Assert.IsTrue(queriedInstances.Any(q => q.Id == sourceInstance.Id && q.Data == sourceInstance.Data));
			}
		}

		private class BasicPocoWithGuidId
		{
			public Guid Id { get; private set; }
			public string Data { get; set; }
		}
	}
}