using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class ManyToManyTests
	{
		private EntityModel<PocoWithManyRelationships> _conventionDrivenModel =
			TestDb.CreateDomainAndModel<PocoWithManyRelationships>(builder => {
				builder.AddDataEntity<RelationshipTypeA>();
				builder.AddDataEntity<RelationshipTypeB>();
			});

		[TestMethod]
		public void ConventionDrivenModelModelsManyToMany()
		{
			var model = _conventionDrivenModel;
			Assert.AreEqual(3, model.Fields.Length);
			Assert.AreEqual(3, model.Schema.Tables.Count);

			var fieldForRelationshipA = model.Fields.FirstOrDefault(q => q.Name == "RelationshipA");
			var fieldForRelationshipB = model.Fields.FirstOrDefault(q => q.Name == "RelationshipB");

			Assert.IsNotNull(fieldForRelationshipA);
			Assert.IsNotNull(fieldForRelationshipA.Relationship);
			Assert.ReferenceEquals(model.Domain.GetEntityModel<RelationshipTypeA>(), fieldForRelationshipA.Relationship.ForeignModel);
			Assert.AreEqual(RelationshipType.ManyToMany, fieldForRelationshipA.Relationship.RelationshipType);
			Assert.IsNull(fieldForRelationshipA.Storage);

			Assert.IsNotNull(fieldForRelationshipB);
			Assert.IsNotNull(fieldForRelationshipB.Relationship);
			Assert.ReferenceEquals(model.Domain.GetEntityModel<RelationshipTypeB>(), fieldForRelationshipB.Relationship.ForeignModel);
			Assert.AreEqual(RelationshipType.ManyToMany, fieldForRelationshipB.Relationship.RelationshipType);
			Assert.IsNull(fieldForRelationshipB.Storage);

			Assert.AreEqual(1, model.Schema.EntityTable.DataFields.Count);
			var idField = model.Schema.EntityTable.DataFields.FirstOrDefault(q => q.Name == "Id");
			Assert.IsNotNull(idField);
			Assert.AreEqual(typeof(Guid), idField.DataType);
			Assert.IsTrue(idField.Storage.IsAutoGenerate);
			Assert.IsTrue(idField.Storage.IsPrimaryKey);

			var relationshipATable = model.Schema.RelationshipTables.FirstOrDefault(q => q.TableName == "PocoWithManyRelationshipsToRelationshipTypeA");
			Assert.AreEqual(2, relationshipATable.DataFields.Count);
			var modelIdField = relationshipATable.DataFields.FirstOrDefault(q => q.Name == "PocoWithManyRelationships_Id");
			Assert.IsNotNull(modelIdField);
			Assert.AreEqual(typeof(Guid), modelIdField.DataType);
			var relationshipIdField = relationshipATable.DataFields.FirstOrDefault(q => q.Name == "RelationshipTypeA_Id");
			Assert.IsNotNull(relationshipIdField);
			Assert.AreEqual(typeof(int), relationshipIdField.DataType);

			var relationshipBTable = model.Schema.RelationshipTables.FirstOrDefault(q => q.TableName == "PocoWithManyRelationshipsToRelationshipTypeB");
			Assert.AreEqual(2, relationshipBTable.DataFields.Count);
			modelIdField = relationshipBTable.DataFields.FirstOrDefault(q => q.Name == "PocoWithManyRelationships_Id");
			Assert.IsNotNull(modelIdField);
			Assert.AreEqual(typeof(Guid), modelIdField.DataType);
			relationshipIdField = relationshipBTable.DataFields.FirstOrDefault(q => q.Name == "RelationshipTypeB_Id");
			Assert.IsNotNull(relationshipIdField);
			Assert.AreEqual(typeof(Guid), relationshipIdField.DataType);
		}

		[TestMethod]
		public async Task InsertManyToManyNullRelationships()
		{
			var model = _conventionDrivenModel;
			var domain = model.Domain;

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var mainInstance = new PocoWithManyRelationships
				{
					RelationshipA = null,
					RelationshipB = null
				};

				await domain
					.Insert(mainInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(model.Schema.EntityTable.TableName),
						joins: new[]
						{
							QueryExpression.Join(
								QueryExpression.Column("Id", QueryExpression.Table(model.Schema.EntityTable.TableName)),
								QueryExpression.Column("PocoWithManyRelationships_Id", QueryExpression.Table("PocoWithManyRelationshipsToRelationshipTypeA")),
								JoinDirection.Left
								)
						}
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());
					Assert.IsTrue(queryResult.IsDBNull(2));
				}

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(model.Schema.EntityTable.TableName),
						joins: new[]
						{
							QueryExpression.Join(
								QueryExpression.Column("Id", QueryExpression.Table(model.Schema.EntityTable.TableName)),
								QueryExpression.Column("PocoWithManyRelationships_Id", QueryExpression.Table("PocoWithManyRelationshipsToRelationshipTypeB")),
								JoinDirection.Left
								)
						}
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());
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
		public async Task InsertManyToMany()
		{
			var model = _conventionDrivenModel;
			var domain = model.Domain;

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var mainInstance = new PocoWithManyRelationships
				{
					RelationshipA = new List<RelationshipTypeA>
					{
						new RelationshipTypeA { Data = 10 },
						new RelationshipTypeA { Data = 20 },
						new RelationshipTypeA { Data = 30 }
					},
					RelationshipB = new RelationshipTypeB[]
					{
						new RelationshipTypeB { Data = 40 },
						new RelationshipTypeB { Data = 50 },
						new RelationshipTypeB { Data = 60 }
					}
				};

				await domain
					.Insert(mainInstance.RelationshipA)
					.Insert(mainInstance.RelationshipB)
					.Insert(mainInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(model.Schema.EntityTable.TableName),
						joins: new[]
						{
							QueryExpression.Join(
								QueryExpression.Column("Id", QueryExpression.Table(model.Schema.EntityTable.TableName)),
								QueryExpression.Column("PocoWithManyRelationships_Id", QueryExpression.Table("PocoWithManyRelationshipsToRelationshipTypeA")),
								JoinDirection.Left
								)
						}
					)))
				{
					var ids = new int[3];

					Assert.IsTrue(queryResult.HasRows);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[0] = queryResult.GetInt32(2);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[1] = queryResult.GetInt32(2);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[2] = queryResult.GetInt32(2);

					Assert.IsFalse(await queryResult.ReadAsync());

					foreach (var relationship in mainInstance.RelationshipA)
					{
						Assert.IsTrue(ids.Contains(relationship.Id));
					}
				}

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(model.Schema.EntityTable.TableName),
						joins: new[]
						{
							QueryExpression.Join(
								QueryExpression.Column("Id", QueryExpression.Table(model.Schema.EntityTable.TableName)),
								QueryExpression.Column("PocoWithManyRelationships_Id", QueryExpression.Table("PocoWithManyRelationshipsToRelationshipTypeB")),
								JoinDirection.Left
								)
						}
					)))
				{
					var ids = new Guid[3];

					Assert.IsTrue(queryResult.HasRows);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[0] = queryResult.GetGuid(2);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[1] = queryResult.GetGuid(2);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[2] = queryResult.GetGuid(2);

					Assert.IsFalse(await queryResult.ReadAsync());

					foreach (var relationship in mainInstance.RelationshipB)
					{
						Assert.IsTrue(ids.Contains(relationship.Id));
					}
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
		public async Task InsertViewWithFullModelManyToMany()
		{
			var model = _conventionDrivenModel;
			var domain = model.Domain;

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var mainInstance = new PocoWithManyRelationshipsViewWithRelationshipA
				{
					RelationshipA = new List<RelationshipTypeA>
					{
						new RelationshipTypeA { Data = 10 },
						new RelationshipTypeA { Data = 20 },
						new RelationshipTypeA { Data = 30 }
					}
				};

				await domain
					.Insert(mainInstance.RelationshipA)
					.Insert<PocoWithManyRelationships, PocoWithManyRelationshipsViewWithRelationshipA>(mainInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(model.Schema.EntityTable.TableName),
						joins: new[]
						{
							QueryExpression.Join(
								QueryExpression.Column("Id", QueryExpression.Table(model.Schema.EntityTable.TableName)),
								QueryExpression.Column("PocoWithManyRelationships_Id", QueryExpression.Table("PocoWithManyRelationshipsToRelationshipTypeA")),
								JoinDirection.Left
								)
						}
					)))
				{
					var ids = new int[3];

					Assert.IsTrue(queryResult.HasRows);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[0] = queryResult.GetInt32(2);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[1] = queryResult.GetInt32(2);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[2] = queryResult.GetInt32(2);

					Assert.IsFalse(await queryResult.ReadAsync());

					foreach (var relationship in mainInstance.RelationshipA)
					{
						Assert.IsTrue(ids.Contains(relationship.Id));
					}
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
		public async Task InsertViewWithSubViewManyToMany()
		{
			var model = _conventionDrivenModel;
			var domain = model.Domain;

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var mainInstance = new PocoWithManyRelationshipsViewWithRelationshipAView
				{
					RelationshipA = new RelationshipTypeAView[]
					{
						new RelationshipTypeAView { Data = 10 },
						new RelationshipTypeAView { Data = 20 },
						new RelationshipTypeAView { Data = 30 }
					}
				};

				await domain
					.Insert<RelationshipTypeA, RelationshipTypeAView>(mainInstance.RelationshipA)
					.Insert<PocoWithManyRelationships, PocoWithManyRelationshipsViewWithRelationshipAView>(mainInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(model.Schema.EntityTable.TableName),
						joins: new[]
						{
							QueryExpression.Join(
								QueryExpression.Column("Id", QueryExpression.Table(model.Schema.EntityTable.TableName)),
								QueryExpression.Column("PocoWithManyRelationships_Id", QueryExpression.Table("PocoWithManyRelationshipsToRelationshipTypeA")),
								JoinDirection.Left
								)
						}
					)))
				{
					var ids = new int[3];

					Assert.IsTrue(queryResult.HasRows);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[0] = queryResult.GetInt32(2);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[1] = queryResult.GetInt32(2);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[2] = queryResult.GetInt32(2);

					Assert.IsFalse(await queryResult.ReadAsync());

					foreach (var relationship in mainInstance.RelationshipA)
					{
						Assert.IsTrue(ids.Contains(relationship.Id));
					}
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
		public async Task UpdateManyToManyNullRelationships()
		{
			var model = _conventionDrivenModel;
			var domain = model.Domain;

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var mainInstance = new PocoWithManyRelationships
				{
					RelationshipA = new List<RelationshipTypeA>
					{
						new RelationshipTypeA { Data = 10 },
						new RelationshipTypeA { Data = 20 },
						new RelationshipTypeA { Data = 30 }
					},
					RelationshipB = new RelationshipTypeB[]
					{
						new RelationshipTypeB { Data = 40 },
						new RelationshipTypeB { Data = 50 },
						new RelationshipTypeB { Data = 60 }
					}
				};

				await domain
					.Insert(mainInstance.RelationshipA)
					.Insert(mainInstance.RelationshipB)
					.Insert(mainInstance)
					.ExecuteAsync(TestDb.Provider);

				mainInstance.RelationshipA = null;
				mainInstance.RelationshipB = null;

				await domain.Update(mainInstance).ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(model.Schema.EntityTable.TableName),
						joins: new[]
						{
							QueryExpression.Join(
								QueryExpression.Column("Id", QueryExpression.Table(model.Schema.EntityTable.TableName)),
								QueryExpression.Column("PocoWithManyRelationships_Id", QueryExpression.Table("PocoWithManyRelationshipsToRelationshipTypeA")),
								JoinDirection.Left
								)
						}
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());
					Assert.IsTrue(queryResult.IsDBNull(2));
				}

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(model.Schema.EntityTable.TableName),
						joins: new[]
						{
							QueryExpression.Join(
								QueryExpression.Column("Id", QueryExpression.Table(model.Schema.EntityTable.TableName)),
								QueryExpression.Column("PocoWithManyRelationships_Id", QueryExpression.Table("PocoWithManyRelationshipsToRelationshipTypeB")),
								JoinDirection.Left
								)
						}
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());
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
		public async Task UpdateManyToMany()
		{
			var model = _conventionDrivenModel;
			var domain = model.Domain;

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var mainInstance = new PocoWithManyRelationships
				{
					RelationshipA = new List<RelationshipTypeA>
					{
						new RelationshipTypeA { Data = 10 },
						new RelationshipTypeA { Data = 20 },
						new RelationshipTypeA { Data = 30 }
					},
					RelationshipB = new RelationshipTypeB[]
					{
						new RelationshipTypeB { Data = 40 },
						new RelationshipTypeB { Data = 50 },
						new RelationshipTypeB { Data = 60 }
					}
				};

				await domain
					.Insert(mainInstance.RelationshipA)
					.Insert(mainInstance.RelationshipB)
					.Insert(mainInstance)
					.ExecuteAsync(TestDb.Provider);

				mainInstance.RelationshipA = new List<RelationshipTypeA>
				{
					new RelationshipTypeA { Data = 15 },
					new RelationshipTypeA { Data = 25 },
					new RelationshipTypeA { Data = 35 }
				};
				mainInstance.RelationshipB = new RelationshipTypeB[]
				{
					new RelationshipTypeB { Data = 45 },
					new RelationshipTypeB { Data = 55 },
					new RelationshipTypeB { Data = 65 }
				};

				await domain
					.Insert(mainInstance.RelationshipA)
					.Insert(mainInstance.RelationshipB)
					.Update(mainInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(model.Schema.EntityTable.TableName),
						joins: new[]
						{
							QueryExpression.Join(
								QueryExpression.Column("Id", QueryExpression.Table(model.Schema.EntityTable.TableName)),
								QueryExpression.Column("PocoWithManyRelationships_Id", QueryExpression.Table("PocoWithManyRelationshipsToRelationshipTypeA")),
								JoinDirection.Left
								)
						}
					)))
				{
					var ids = new int[3];

					Assert.IsTrue(queryResult.HasRows);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[0] = queryResult.GetInt32(2);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[1] = queryResult.GetInt32(2);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[2] = queryResult.GetInt32(2);

					Assert.IsFalse(await queryResult.ReadAsync());

					foreach (var relationship in mainInstance.RelationshipA)
					{
						Assert.IsTrue(ids.Contains(relationship.Id));
					}
				}

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(model.Schema.EntityTable.TableName),
						joins: new[]
						{
							QueryExpression.Join(
								QueryExpression.Column("Id", QueryExpression.Table(model.Schema.EntityTable.TableName)),
								QueryExpression.Column("PocoWithManyRelationships_Id", QueryExpression.Table("PocoWithManyRelationshipsToRelationshipTypeB")),
								JoinDirection.Left
								)
						}
					)))
				{
					var ids = new Guid[3];

					Assert.IsTrue(queryResult.HasRows);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[0] = queryResult.GetGuid(2);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[1] = queryResult.GetGuid(2);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[2] = queryResult.GetGuid(2);

					Assert.IsFalse(await queryResult.ReadAsync());

					foreach (var relationship in mainInstance.RelationshipB)
					{
						Assert.IsTrue(ids.Contains(relationship.Id));
					}
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
		public async Task UpdateViewWithFullModelManyToMany()
		{
			var model = _conventionDrivenModel;
			var domain = model.Domain;

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var mainInstance = new PocoWithManyRelationshipsViewWithRelationshipA
				{
					RelationshipA = new List<RelationshipTypeA>
					{
						new RelationshipTypeA { Data = 10 },
						new RelationshipTypeA { Data = 20 },
						new RelationshipTypeA { Data = 30 }
					}
				};

				await domain
					.Insert(mainInstance.RelationshipA)
					.Insert<PocoWithManyRelationships, PocoWithManyRelationshipsViewWithRelationshipA>(mainInstance)
					.ExecuteAsync(TestDb.Provider);

				mainInstance.RelationshipA = new List<RelationshipTypeA>
				{
					new RelationshipTypeA { Data = 15 },
					new RelationshipTypeA { Data = 25 },
					new RelationshipTypeA { Data = 35 }
				};

				await domain
					.Insert(mainInstance.RelationshipA)
					.Update<PocoWithManyRelationships, PocoWithManyRelationshipsViewWithRelationshipA>(mainInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(model.Schema.EntityTable.TableName),
						joins: new[]
						{
							QueryExpression.Join(
								QueryExpression.Column("Id", QueryExpression.Table(model.Schema.EntityTable.TableName)),
								QueryExpression.Column("PocoWithManyRelationships_Id", QueryExpression.Table("PocoWithManyRelationshipsToRelationshipTypeA")),
								JoinDirection.Left
								)
						}
					)))
				{
					var ids = new int[3];

					Assert.IsTrue(queryResult.HasRows);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[0] = queryResult.GetInt32(2);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[1] = queryResult.GetInt32(2);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[2] = queryResult.GetInt32(2);

					Assert.IsFalse(await queryResult.ReadAsync());

					foreach (var relationship in mainInstance.RelationshipA)
					{
						Assert.IsTrue(ids.Contains(relationship.Id));
					}
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
		public async Task UpdateViewWithSubViewManyToMany()
		{
			var model = _conventionDrivenModel;
			var domain = model.Domain;

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var mainInstance = new PocoWithManyRelationshipsViewWithRelationshipAView
				{
					RelationshipA = new RelationshipTypeAView[]
					{
						new RelationshipTypeAView { Data = 10 },
						new RelationshipTypeAView { Data = 20 },
						new RelationshipTypeAView { Data = 30 }
					}
				};

				await domain
					.Insert<RelationshipTypeA, RelationshipTypeAView>(mainInstance.RelationshipA)
					.Insert<PocoWithManyRelationships, PocoWithManyRelationshipsViewWithRelationshipAView>(mainInstance)
					.ExecuteAsync(TestDb.Provider);

				mainInstance.RelationshipA = new RelationshipTypeAView[]
				{
					new RelationshipTypeAView { Data = 15 },
					new RelationshipTypeAView { Data = 25 },
					new RelationshipTypeAView { Data = 35 }
				};

				await domain
					.Insert<RelationshipTypeA, RelationshipTypeAView>(mainInstance.RelationshipA)
					.Update<PocoWithManyRelationships, PocoWithManyRelationshipsViewWithRelationshipAView>(mainInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(model.Schema.EntityTable.TableName),
						joins: new[]
						{
							QueryExpression.Join(
								QueryExpression.Column("Id", QueryExpression.Table(model.Schema.EntityTable.TableName)),
								QueryExpression.Column("PocoWithManyRelationships_Id", QueryExpression.Table("PocoWithManyRelationshipsToRelationshipTypeA")),
								JoinDirection.Left
								)
						}
					)))
				{
					var ids = new int[3];

					Assert.IsTrue(queryResult.HasRows);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[0] = queryResult.GetInt32(2);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[1] = queryResult.GetInt32(2);

					Assert.IsTrue(await queryResult.ReadAsync());
					ids[2] = queryResult.GetInt32(2);

					Assert.IsFalse(await queryResult.ReadAsync());

					foreach (var relationship in mainInstance.RelationshipA)
					{
						Assert.IsTrue(ids.Contains(relationship.Id));
					}
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
		public async Task DeleteManyToManyRelationships()
		{
			var model = _conventionDrivenModel;
			var domain = model.Domain;

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var mainInstance = new PocoWithManyRelationships
				{
					RelationshipA = new List<RelationshipTypeA>
					{
						new RelationshipTypeA { Data = 10 },
						new RelationshipTypeA { Data = 20 },
						new RelationshipTypeA { Data = 30 }
					},
					RelationshipB = new RelationshipTypeB[]
					{
						new RelationshipTypeB { Data = 40 },
						new RelationshipTypeB { Data = 50 },
						new RelationshipTypeB { Data = 60 }
					}
				};

				await domain
					.Insert(mainInstance.RelationshipA)
					.Insert(mainInstance.RelationshipB)
					.Insert(mainInstance)
					.ExecuteAsync(TestDb.Provider);

				await domain.Delete(mainInstance).ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table("PocoWithManyRelationshipsToRelationshipTypeA")
					)))
				{
					Assert.IsFalse(queryResult.HasRows);
				}

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table("PocoWithManyRelationshipsToRelationshipTypeB")
					)))
				{
					Assert.IsFalse(queryResult.HasRows);
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
		public async Task DeleteViewWithFullModelManyToMany()
		{
			var model = _conventionDrivenModel;
			var domain = model.Domain;

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var mainInstance = new PocoWithManyRelationshipsViewWithRelationshipA
				{
					RelationshipA = new List<RelationshipTypeA>
					{
						new RelationshipTypeA { Data = 10 },
						new RelationshipTypeA { Data = 20 },
						new RelationshipTypeA { Data = 30 }
					}
				};

				await domain
					.Insert(mainInstance.RelationshipA)
					.Insert<PocoWithManyRelationships, PocoWithManyRelationshipsViewWithRelationshipA>(mainInstance)
					.ExecuteAsync(TestDb.Provider);

				await domain
					.Delete<PocoWithManyRelationships, PocoWithManyRelationshipsViewWithRelationshipA>(mainInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table("PocoWithManyRelationshipsToRelationshipTypeA")
					)))
				{
					Assert.IsFalse(queryResult.HasRows);
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
		public async Task DeleteViewWithSubViewManyToMany()
		{
			var model = _conventionDrivenModel;
			var domain = model.Domain;

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var mainInstance = new PocoWithManyRelationshipsViewWithRelationshipAView
				{
					RelationshipA = new RelationshipTypeAView[]
					{
						new RelationshipTypeAView { Data = 10 },
						new RelationshipTypeAView { Data = 20 },
						new RelationshipTypeAView { Data = 30 }
					}
				};

				await domain
					.Insert<RelationshipTypeA, RelationshipTypeAView>(mainInstance.RelationshipA)
					.Insert<PocoWithManyRelationships, PocoWithManyRelationshipsViewWithRelationshipAView>(mainInstance)
					.ExecuteAsync(TestDb.Provider);

				await domain
					.Delete<PocoWithManyRelationships, PocoWithManyRelationshipsViewWithRelationshipAView>(mainInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table("PocoWithManyRelationshipsToRelationshipTypeA")
					)))
				{
					Assert.IsFalse(queryResult.HasRows);
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
		public async Task SelectManyToManyNullRelationships()
		{
			var model = _conventionDrivenModel;
			var domain = model.Domain;

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var mainInstance = new PocoWithManyRelationships
				{
					RelationshipA = null,
					RelationshipB = null
				};

				await domain
					.Insert(mainInstance)
					.ExecuteAsync(TestDb.Provider);

				var queriedInstance =
					(await domain.Select<PocoWithManyRelationships>()
					.ExecuteAsync(TestDb.Provider))
					.FirstOrDefault();

				Assert.IsNotNull(queriedInstance);
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
		public async Task SelectManyToMany()
		{
			var model = _conventionDrivenModel;
			var domain = model.Domain;

			foreach (var entityModel in model.Domain.DataModels)
			{
				foreach (var table in entityModel.Schema.Tables)
				{
					await table.CreateAsync(TestDb.Provider);
				}
			}

			try
			{
				var mainInstance = new PocoWithManyRelationships
				{
					RelationshipA = new List<RelationshipTypeA>
					{
						new RelationshipTypeA { Data = 10 },
						new RelationshipTypeA { Data = 20 },
						new RelationshipTypeA { Data = 30 }
					},
					RelationshipB = new RelationshipTypeB[]
					{
						new RelationshipTypeB { Data = 40 },
						new RelationshipTypeB { Data = 50 },
						new RelationshipTypeB { Data = 60 }
					}
				};

				await domain
					.Insert(mainInstance.RelationshipA)
					.Insert(mainInstance.RelationshipB)
					.Insert(mainInstance)
					.ExecuteAsync(TestDb.Provider);

				var queriedInstance =
					(await domain.Select<PocoWithManyRelationships>()
					.ExecuteAsync(TestDb.Provider))
					.FirstOrDefault();

				Assert.IsNotNull(queriedInstance);
				Assert.IsNotNull(queriedInstance.RelationshipA);
				Assert.IsNotNull(queriedInstance.RelationshipB);
				Assert.AreEqual(mainInstance.RelationshipA.Count, queriedInstance.RelationshipA.Count);
				Assert.AreEqual(mainInstance.RelationshipB.Length, queriedInstance.RelationshipB.Length);

				foreach (var relatedObj in mainInstance.RelationshipA)
				{
					Assert.IsTrue(queriedInstance.RelationshipA.Any(q => q.Id == relatedObj.Id && q.Data == relatedObj.Data));
				}

				foreach (var relatedObj in mainInstance.RelationshipB)
				{
					Assert.IsTrue(queriedInstance.RelationshipB.Any(q => q.Id == relatedObj.Id && q.Data == relatedObj.Data));
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

		private class PocoWithManyRelationships
		{
			public Guid Id { get; private set; }
			public List<RelationshipTypeA> RelationshipA { get; set; } = new List<RelationshipTypeA>();
			public RelationshipTypeB[] RelationshipB { get; set; } = new RelationshipTypeB[0];
		}

		private class PocoWithManyRelationshipsViewWithRelationshipA
		{
			public Guid Id { get; private set; }
			public List<RelationshipTypeA> RelationshipA { get; set; } = new List<RelationshipTypeA>();
		}

		private class PocoWithManyRelationshipsViewWithRelationshipAView
		{
			public Guid Id { get; private set; }
			public RelationshipTypeAView[] RelationshipA { get; set; }
		}

		private class RelationshipTypeA
		{
			public int Id { get; private set; }
			public int Data { get; set; }
		}

		private class RelationshipTypeB
		{
			public Guid Id { get; private set; }
			public int Data { get; set; }
		}

		private class RelationshipTypeAView
		{
			public int Id { get; private set; }
			public int Data { get; set; }
		}
	}
}
