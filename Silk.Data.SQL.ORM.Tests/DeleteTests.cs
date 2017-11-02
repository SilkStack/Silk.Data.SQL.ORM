using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class DeleteTests
	{
		[TestMethod]
		public void DeleteSimpleModel()
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
			dataModel.Delete(sourceInstances)
				.Execute(TestDb.Provider);

			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table(dataModel.Schema.Tables.First().TableName),
					where: QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.None, QueryExpression.InFunction(sourceInstances.Select(q => (object)q.Id).ToArray()))
				)))
			{
				Assert.IsFalse(queryResult.HasRows);
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
	}
}
