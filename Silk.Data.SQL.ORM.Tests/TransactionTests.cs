using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Queries;
using System;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class TransactionTests
	{
		private static readonly EntityModel<ModelWithRelationships> _conventionDrivenModel =
			TestDb.CreateDomainAndModel<ModelWithRelationships>(builder =>
			{
				builder.AddDataEntity<RelationshipModelA>();
				builder.AddDataEntity<RelationshipModelB>();
			});

		[TestMethod]
		public async Task InsertRollsbackOnError()
		{
			var model = TestDb.CreateDomainAndModel<RelationshipModelA>();

			await model.Schema.EntityTable.CreateAsync(TestDb.Provider);

			try
			{
				try
				{
					await model.Domain
						.Insert(new RelationshipModelA { Data = 60 })
						.NonResultQuery(new FailingORMQuery())
						.AsTransaction()
						.ExecuteAsync(TestDb.Provider);
				}
				catch (Exception) { }

				var rows = await model.Domain
					.Select<RelationshipModelA>()
					.ExecuteAsync(TestDb.Provider);
				Assert.AreEqual(0, rows.Count);
			}
			finally
			{
				await model.Schema.EntityTable.DropAsync(TestDb.Provider);
			}
		}

		[TestMethod]
		public async Task InsertManyToOneAsTransaction()
		{
			var model = _conventionDrivenModel;
			var relationshipAModel = _conventionDrivenModel.Domain.GetEntityModel<RelationshipModelA>();
			var relationshipBModel = _conventionDrivenModel.Domain.GetEntityModel<RelationshipModelB>();

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var objInstance = new ModelWithRelationships
				{
					RelationshipA = new RelationshipModelA { Data = 10 },
					RelationshipB = new RelationshipModelB { Data = 20 }
				};

				await model.Domain
					.Insert(objInstance.RelationshipA)
					.Insert(objInstance.RelationshipB)
					.Insert(objInstance)
					.AsTransaction()
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(model.Schema.EntityTable.TableName),
						joins: new[]
						{
							QueryExpression.Join(
								QueryExpression.Column("RelationshipAId", QueryExpression.Table(model.Schema.EntityTable.TableName)),
								QueryExpression.Column("Id", QueryExpression.Table("RelationshipModelA")),
								JoinDirection.Left
								),
							QueryExpression.Join(
								QueryExpression.Column("RelationshipBId", QueryExpression.Table(model.Schema.EntityTable.TableName)),
								QueryExpression.Column("Id", QueryExpression.Table("RelationshipModelB")),
								JoinDirection.Left
								)
						}
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());
					Assert.AreEqual(objInstance.Id, queryResult.GetGuid(0));
					Assert.AreEqual(objInstance.RelationshipA.Id, queryResult.GetGuid(1));
					Assert.AreEqual(objInstance.RelationshipB.Id, queryResult.GetInt32(2));

					Assert.AreEqual(objInstance.RelationshipA.Id, queryResult.GetGuid(3));
					Assert.AreEqual(objInstance.RelationshipA.Data, queryResult.GetInt32(4));

					Assert.AreEqual(objInstance.RelationshipB.Id, queryResult.GetInt32(5));
					Assert.AreEqual(objInstance.RelationshipB.Data, queryResult.GetInt32(6));
				}
			}
			finally
			{
				foreach (var entityModel in model.Domain.DataModels)
				{
					foreach (var table in entityModel.Schema.Tables)
					{
						await table.DropAsync(TestDb.Provider);
					}
				}
			}
		}

		private class ModelWithRelationships
		{
			public Guid Id { get; private set; }
			public RelationshipModelA RelationshipA { get; set; }
			public RelationshipModelB RelationshipB { get; set; }
		}

		private class RelationshipModelA
		{
			public Guid Id { get; private set; }
			public int Data { get; set; }
		}

		private class RelationshipModelB
		{
			public int Id { get; private set; }
			public int Data { get; set; }
		}

		private class FailingORMQuery : ORMQuery
		{
			public FailingORMQuery()
				: base(QueryExpression.Select(
					new [] { QueryExpression.All() },
					from: QueryExpression.Table("ThisTableNeverExists")
					), null, false)
			{
			}
		}
	}
}
