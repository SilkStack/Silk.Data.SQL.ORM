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
			var entitySchema = TestDb.CreateDomainAndSchema<BasicPocoWithGuidId>();
			var table = entitySchema.EntityTable;
			var database = new EntityDatabase<BasicPocoWithGuidId>(entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var sourceInstances = new[]
				{
					new BasicPocoWithGuidId { Data = "Hello World 1" },
					new BasicPocoWithGuidId { Data = "Hello World 2" },
					new BasicPocoWithGuidId { Data = "Hello World 3" }
				};
				database.Insert(sourceInstances).Execute();
				database.Delete(sourceInstances).Execute();

				using (var queryResult = TestDb.Provider.ExecuteReader(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(table.TableName),
						where: QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.None, QueryExpression.InFunction(sourceInstances.Select(q => (object)q.Id).ToArray()))
					)))
				{
					Assert.IsFalse(queryResult.HasRows);
				}
			}
			finally
			{
				TestDb.ExecuteSql(table.DropTable());
			}
		}

		[TestMethod]
		public async Task DeleteAll()
		{
			var entitySchema = TestDb.CreateDomainAndSchema<BasicPocoWithGuidId>();
			var table = entitySchema.EntityTable;
			var database = new EntityDatabase<BasicPocoWithGuidId>(entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var sourceInstances = new[]
				{
					new BasicPocoWithGuidId { Data = "Hello World 1" },
					new BasicPocoWithGuidId { Data = "Hello World 2" },
					new BasicPocoWithGuidId { Data = "Hello World 3" }
				};
				await database.Insert(sourceInstances).ExecuteAsync();
				await database.Delete().ExecuteAsync();

				using (var queryResult = TestDb.Provider.ExecuteReader(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(table.TableName),
						where: QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.None, QueryExpression.InFunction(sourceInstances.Select(q => (object)q.Id).ToArray()))
					)))
				{
					Assert.IsFalse(queryResult.HasRows);
				}
			}
			finally
			{
				TestDb.ExecuteSql(table.DropTable());
			}
		}

		[TestMethod]
		public async Task DeleteWithWhere()
		{
			var entitySchema = TestDb.CreateDomainAndSchema<BasicPocoWithGuidId>();
			var table = entitySchema.EntityTable;
			var database = new EntityDatabase<BasicPocoWithGuidId>(entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var sourceInstances = new[]
				{
					new BasicPocoWithGuidId { Data = "Hello World 1" },
					new BasicPocoWithGuidId { Data = "Hello World 2" },
					new BasicPocoWithGuidId { Data = "Hello World 3" }
				};
				await database.Insert(sourceInstances).ExecuteAsync();
				await database.Delete(
					where: QueryExpression.Compare(
						QueryExpression.Column("Id"),
						ComparisonOperator.AreEqual,
						QueryExpression.Value(sourceInstances[0].Id)
					)).ExecuteAsync();

				using (var queryResult = TestDb.Provider.ExecuteReader(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(table.TableName),
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
				TestDb.ExecuteSql(table.DropTable());
			}
		}

		private class BasicPocoWithGuidId
		{
			public Guid Id { get; private set; }
			public string Data { get; set; }
		}
	}
}
