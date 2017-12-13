using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using System;
using System.Linq;
using System.Threading.Tasks;

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

			try
			{
				var sourceInstances = new[]
				{
					new BasicPocoWithGuidId { Data = "Hello World 1" },
					new BasicPocoWithGuidId { Data = "Hello World 2" },
					new BasicPocoWithGuidId { Data = "Hello World 3" }
				};
				dataModel.Domain.Insert(sourceInstances)
					.Execute(TestDb.Provider);
				dataModel.Domain.Delete(sourceInstances)
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
			}
			finally
			{
				foreach (var table in dataModel.Schema.Tables)
				{
					table.Drop(TestDb.Provider);
				}
			}
		}

		[TestMethod]
		public async Task DeleteAll()
		{
			var dataModel = TestDb.CreateDomainAndModel<BasicPocoWithGuidId>();

			foreach (var table in dataModel.Schema.Tables)
			{
				if (!table.Exists(TestDb.Provider))
					await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var sourceInstances = new[]
				{
					new BasicPocoWithGuidId { Data = "Hello World 1" },
					new BasicPocoWithGuidId { Data = "Hello World 2" },
					new BasicPocoWithGuidId { Data = "Hello World 3" }
				};
				await dataModel.Domain.Insert(sourceInstances)
					.ExecuteAsync(TestDb.Provider);
				await dataModel.Domain.Delete<BasicPocoWithGuidId>()
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = TestDb.Provider.ExecuteReader(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.Tables.First().TableName),
						where: QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.None, QueryExpression.InFunction(sourceInstances.Select(q => (object)q.Id).ToArray()))
					)))
				{
					Assert.IsFalse(queryResult.HasRows);
				}
			}
			finally
			{
				foreach (var table in dataModel.Schema.Tables)
				{
					await table.DropAsync(TestDb.Provider);
				}
			}
		}

		[TestMethod]
		public async Task DeleteWithWhere()
		{
			var dataModel = TestDb.CreateDomainAndModel<BasicPocoWithGuidId>();

			foreach (var table in dataModel.Schema.Tables)
			{
				if (!table.Exists(TestDb.Provider))
					await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var sourceInstances = new[]
				{
					new BasicPocoWithGuidId { Data = "Hello World 1" },
					new BasicPocoWithGuidId { Data = "Hello World 2" },
					new BasicPocoWithGuidId { Data = "Hello World 3" }
				};
				await dataModel.Domain.Insert(sourceInstances)
					.ExecuteAsync(TestDb.Provider);
				await dataModel.Domain.Delete<BasicPocoWithGuidId>(
					QueryExpression.Compare(
						QueryExpression.Column("Id"),
						ComparisonOperator.AreEqual,
						QueryExpression.Value(sourceInstances[0].Id)
					)).ExecuteAsync(TestDb.Provider);

				using (var queryResult = TestDb.Provider.ExecuteReader(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.Tables.First().TableName),
						where: QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.None, QueryExpression.InFunction(sourceInstances.Select(q => (object)q.Id).ToArray()))
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());
					Assert.AreNotEqual(sourceInstances[0].Id, queryResult.GetGuid(0));
					Assert.IsTrue(await queryResult.ReadAsync());
					Assert.AreNotEqual(sourceInstances[0].Id, queryResult.GetGuid(0));
					Assert.IsFalse(await queryResult.ReadAsync());
				}
			}
			finally
			{
				foreach (var table in dataModel.Schema.Tables)
				{
					await table.DropAsync(TestDb.Provider);
				}
			}
		}

		private class BasicPocoWithGuidId
		{
			public Guid Id { get; private set; }
			public string Data { get; set; }
		}
	}
}
