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
			dataModel.Delete(sourceInstances)
				.Execute(TestDb.Provider); ;

			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table(dataModel.Tables.First().TableName),
					where: QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.None, QueryExpression.InFunction(sourceInstances.Select(q => (object)q.Id).ToArray())),
					orderBy: new[] {
						QueryExpression.Descending(QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.AreEqual, QueryExpression.Value(sourceInstances[0].Id))),
						QueryExpression.Descending(QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.AreEqual, QueryExpression.Value(sourceInstances[1].Id))),
						QueryExpression.Descending(QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.AreEqual, QueryExpression.Value(sourceInstances[2].Id)))
					}
				)))
			{
				Assert.IsFalse(queryResult.HasRows);
			}

			foreach (var table in dataModel.Tables)
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
