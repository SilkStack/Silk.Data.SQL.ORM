using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class ManyToOneTests
	{
		private static readonly EntityModel<ModelWithRelationships> _conventionDrivenModel =
			TestDb.CreateDomainAndModel<ModelWithRelationships>(builder =>
			{
				builder.AddDataEntity<RelationshipModelA>();
				builder.AddDataEntity<RelationshipModelB>();
			});
		private static readonly EntityModel<ModelWithRelationships, ModelWithRelationshipsView> _modelDrivenModel =
			TestDb.CreateDomainAndModel<ModelWithRelationships, ModelWithRelationshipsView>(builder =>
			{
				builder.AddDataEntity<RelationshipModelA>();
				builder.AddDataEntity<RelationshipModelB>();
			});

		[TestMethod]
		public void ConventionDrivenModelManyToOneRelationship()
		{
			var model = _conventionDrivenModel;

			Assert.AreEqual(3, model.Fields.Length);

			var fieldForRelationshipA = model.Fields.FirstOrDefault(q => q.Name == "RelationshipAId");
			var fieldForRelationshipB = model.Fields.FirstOrDefault(q => q.Name == "RelationshipBId");

			Assert.IsNotNull(fieldForRelationshipA);
			Assert.IsNotNull(fieldForRelationshipA.Relationship);
			Assert.ReferenceEquals(model.Domain.GetEntityModel<RelationshipModelA>(), fieldForRelationshipA.Relationship.ForeignModel);
			Assert.AreEqual(RelationshipType.ManyToOne, fieldForRelationshipA.Relationship.RelationshipType);
			Assert.IsTrue(fieldForRelationshipA.Storage.IsNullable);

			Assert.IsNotNull(fieldForRelationshipB);
			Assert.IsNotNull(fieldForRelationshipB.Relationship);
			Assert.ReferenceEquals(model.Domain.GetEntityModel<RelationshipModelB>(), fieldForRelationshipB.Relationship.ForeignModel);
			Assert.AreEqual(RelationshipType.ManyToOne, fieldForRelationshipB.Relationship.RelationshipType);
			Assert.IsTrue(fieldForRelationshipB.Storage.IsNullable);

			Assert.IsTrue(model.Schema.EntityTable.DataFields.Contains(fieldForRelationshipA));
			Assert.IsTrue(model.Schema.EntityTable.DataFields.Contains(fieldForRelationshipB));
		}

		[TestMethod]
		public void ModelDrivenModelManyToOneRelationship()
		{
			var model = _modelDrivenModel;

			Assert.AreEqual(3, model.Fields.Length);

			var fieldForRelationshipA = model.Fields.FirstOrDefault(q => q.Name == "RelationshipAId");
			var fieldForRelationshipB = model.Fields.FirstOrDefault(q => q.Name == "RelationshipBId");

			Assert.IsNotNull(fieldForRelationshipA);
			Assert.IsNotNull(fieldForRelationshipA.Relationship);
			Assert.ReferenceEquals(model.Domain.GetEntityModel<RelationshipModelA>(), fieldForRelationshipA.Relationship.ForeignModel);
			Assert.AreEqual(RelationshipType.ManyToOne, fieldForRelationshipA.Relationship.RelationshipType);
			Assert.IsTrue(fieldForRelationshipA.Storage.IsNullable);

			Assert.IsNotNull(fieldForRelationshipB);
			Assert.IsNotNull(fieldForRelationshipB.Relationship);
			Assert.ReferenceEquals(model.Domain.GetEntityModel<RelationshipModelB>(), fieldForRelationshipB.Relationship.ForeignModel);
			Assert.AreEqual(RelationshipType.ManyToOne, fieldForRelationshipB.Relationship.RelationshipType);
			Assert.IsTrue(fieldForRelationshipB.Storage.IsNullable);

			Assert.IsTrue(model.Schema.EntityTable.DataFields.Contains(fieldForRelationshipA));
			Assert.IsTrue(model.Schema.EntityTable.DataFields.Contains(fieldForRelationshipB));
		}

		[TestMethod]
		public async Task InsertWithNulls()
		{
			var model = _conventionDrivenModel;

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var objInstance = new ModelWithRelationships();
				await model.Domain.Insert(objInstance)
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
					Assert.IsTrue(queryResult.IsDBNull(1));
					Assert.IsTrue(queryResult.IsDBNull(2));
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

		[TestMethod]
		public async Task InsertFullyPopulated()
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

		[TestMethod]
		public async Task UpdateNewRelationship()
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
					.ExecuteAsync(TestDb.Provider);

				objInstance.RelationshipA = null;
				objInstance.RelationshipB = new RelationshipModelB { Data = 30 };
				await model.Domain
					.Insert(objInstance.RelationshipB)
					.Update(objInstance)
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
					Assert.IsTrue(queryResult.IsDBNull(1));

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

		[TestMethod]
		public async Task SelectRelationshipNulls()
		{
			var model = _conventionDrivenModel;

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var objInstance = new ModelWithRelationships();
				await model.Domain.Insert(objInstance)
					.ExecuteAsync(TestDb.Provider);

				var queriedInstance = (await model.Domain.Select<ModelWithRelationships>()
					.ExecuteAsync(TestDb.Provider)).FirstOrDefault();

				Assert.IsNotNull(queriedInstance);
				Assert.AreEqual(objInstance.Id, queriedInstance.Id);
				Assert.IsNull(queriedInstance.RelationshipA);
				Assert.IsNull(queriedInstance.RelationshipB);
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

		[TestMethod]
		public async Task SelectRelationshipObjects()
		{
			var model = _conventionDrivenModel;

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
					.ExecuteAsync(TestDb.Provider);

				var queriedInstance = (await model.Domain.Select<ModelWithRelationships>()
					.ExecuteAsync(TestDb.Provider)).FirstOrDefault();

				Assert.IsNotNull(queriedInstance);
				Assert.AreEqual(objInstance.Id, queriedInstance.Id);
				Assert.IsNotNull(queriedInstance.RelationshipA);
				Assert.IsNotNull(queriedInstance.RelationshipB);
				Assert.AreEqual(objInstance.RelationshipA.Id, queriedInstance.RelationshipA.Id);
				Assert.AreEqual(objInstance.RelationshipB.Id, queriedInstance.RelationshipB.Id);
				Assert.AreEqual(objInstance.RelationshipA.Data, queriedInstance.RelationshipA.Data);
				Assert.AreEqual(objInstance.RelationshipB.Data, queriedInstance.RelationshipB.Data);
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

		[TestMethod]
		public async Task SelectRelationshipIdsWithNulls()
		{
			var model = _conventionDrivenModel;

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var objInstance = new ModelWithRelationships();
				await model.Domain.Insert(objInstance)
					.ExecuteAsync(TestDb.Provider);

				var queriedInstance = (await model.Domain.Select<ModelWithRelationships, ModelWithRelationshipsView>()
					.ExecuteAsync(TestDb.Provider)).FirstOrDefault();

				Assert.IsNotNull(queriedInstance);
				Assert.AreEqual(objInstance.Id, queriedInstance.Id);
				Assert.AreEqual(default(Guid), queriedInstance.RelationshipAId);
				Assert.AreEqual(default(int), queriedInstance.RelationshipBId);
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

		[TestMethod]
		public async Task SelectRelationshipIds()
		{
			var model = _conventionDrivenModel;

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
					.ExecuteAsync(TestDb.Provider);

				var queriedInstance = (await model.Domain.Select<ModelWithRelationships, ModelWithRelationshipsView>()
					.ExecuteAsync(TestDb.Provider)).FirstOrDefault();

				Assert.IsNotNull(queriedInstance);
				Assert.AreEqual(objInstance.Id, queriedInstance.Id);
				Assert.AreEqual(objInstance.RelationshipA.Id, queriedInstance.RelationshipAId);
				Assert.AreEqual(objInstance.RelationshipB.Id, queriedInstance.RelationshipBId);
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

		private class ModelWithRelationshipsView
		{
			public Guid Id { get; private set; }
			public Guid RelationshipAId { get; set; }
			public int RelationshipBId { get; set; }
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
	}
}
