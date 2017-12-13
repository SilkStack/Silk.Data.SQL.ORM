using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class InsertTests
	{
		[TestMethod]
		public void InsertGuidSimpleModel()
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

			Assert.AreNotEqual(sourceInstances[0].Id, Guid.Empty);
			Assert.AreNotEqual(sourceInstances[1].Id, Guid.Empty);
			Assert.AreNotEqual(sourceInstances[2].Id, Guid.Empty);

			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table(dataModel.Schema.Tables.First().TableName),
					where: QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.None, QueryExpression.InFunction(sourceInstances.Select(q => (object)q.Id).ToArray())),
					orderBy: new[] {
						QueryExpression.Descending(QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.AreEqual, QueryExpression.Value(sourceInstances[0].Id))),
						QueryExpression.Descending(QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.AreEqual, QueryExpression.Value(sourceInstances[1].Id))),
						QueryExpression.Descending(QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.AreEqual, QueryExpression.Value(sourceInstances[2].Id)))
					}
				)))
			{
				Assert.IsTrue(queryResult.HasRows);

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(sourceInstances[0].Id, queryResult.GetGuid(0));
				Assert.AreEqual(sourceInstances[0].Data, queryResult.GetString(1));

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(sourceInstances[1].Id, queryResult.GetGuid(0));
				Assert.AreEqual(sourceInstances[1].Data, queryResult.GetString(1));

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(sourceInstances[2].Id, queryResult.GetGuid(0));
				Assert.AreEqual(sourceInstances[2].Data, queryResult.GetString(1));
			}

			foreach (var table in dataModel.Schema.Tables)
			{
				table.Drop(TestDb.Provider);
			}
		}

		[TestMethod]
		public void InsertIntSimpleModel()
		{
			var dataModel = TestDb.CreateDomainAndModel<BasicPocoWithIntId>();

			foreach (var table in dataModel.Schema.Tables)
			{
				if (!table.Exists(TestDb.Provider))
					table.Create(TestDb.Provider);
			}

			var sourceInstances = new[]
			{
				new BasicPocoWithIntId { Data = "Hello World 1" },
				new BasicPocoWithIntId { Data = "Hello World 2" },
				new BasicPocoWithIntId { Data = "Hello World 3" }
			};
			dataModel.Domain.Insert(sourceInstances)
				.Execute(TestDb.Provider);

			Assert.AreNotEqual(sourceInstances[0].Id, 0);
			Assert.AreNotEqual(sourceInstances[1].Id, 0);
			Assert.AreNotEqual(sourceInstances[2].Id, 0);

			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table(dataModel.Schema.Tables.First().TableName),
					where: QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.None, QueryExpression.InFunction(sourceInstances.Select(q => (object)q.Id).ToArray())),
					orderBy: new[] {
						QueryExpression.Descending(QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.AreEqual, QueryExpression.Value(sourceInstances[0].Id))),
						QueryExpression.Descending(QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.AreEqual, QueryExpression.Value(sourceInstances[1].Id))),
						QueryExpression.Descending(QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.AreEqual, QueryExpression.Value(sourceInstances[2].Id)))
					}
				)))
			{
				Assert.IsTrue(queryResult.HasRows);

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(sourceInstances[0].Id, queryResult.GetInt32(0));
				Assert.AreEqual(sourceInstances[0].Data, queryResult.GetString(1));

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(sourceInstances[1].Id, queryResult.GetInt32(0));
				Assert.AreEqual(sourceInstances[1].Data, queryResult.GetString(1));

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(sourceInstances[2].Id, queryResult.GetInt32(0));
				Assert.AreEqual(sourceInstances[2].Data, queryResult.GetString(1));
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

		private class BasicPocoWithIntId
		{
			public int Id { get; private set; }
			public string Data { get; set; }
		}
	}
}
