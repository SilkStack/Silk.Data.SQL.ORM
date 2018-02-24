using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.NewModelling;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class FlattenEmbeddingTests
	{
		private static EntitySchema<ObjectWithPocoSubModels> _entitySchema =
			TestDb.CreateDomainAndSchema<ObjectWithPocoSubModels>();

		[TestMethod]
		public void FlattenPocoInDataModel()
		{
			var dataModel = _entitySchema;

			Assert.AreEqual(4, dataModel.Fields.Length);
			Assert.IsTrue(dataModel.Fields.Any(
				q => q.Name == "Id" && q.DataType == typeof(Guid) &&
					q.ModelBinding.ModelFieldPath.SequenceEqual(new[] { "Id" })
				));
			Assert.IsTrue(dataModel.Fields.Any(
				q => q.Name == "ModelA_Data" && q.DataType == typeof(string) &&
					q.ModelBinding.ModelFieldPath.SequenceEqual(new[] { "ModelA", "Data" })
				));
			Assert.IsTrue(dataModel.Fields.Any(
				q => q.Name == "ModelB1_Data" && q.DataType == typeof(int) &&
					q.ModelBinding.ModelFieldPath.SequenceEqual(new[] { "ModelB1", "Data" })
				));
			Assert.IsTrue(dataModel.Fields.Any(
				q => q.Name == "ModelB2_Data" && q.DataType == typeof(int) &&
					q.ModelBinding.ModelFieldPath.SequenceEqual(new[] { "ModelB2", "Data" })
				));
		}

		[TestMethod]
		public async Task InsertWithoutNulls()
		{
			var dataModel = _entitySchema;
			var table = _entitySchema.EntityTable;
			var database = new EntityDatabase<ObjectWithPocoSubModels>(_entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await database.Insert(modelInstance)
					.ExecuteAsync();

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(table.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.AreEqual(modelInstance.ModelA.Data, queryResult.GetString(queryResult.GetOrdinal("ModelA_Data")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1_Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2_Data")));
				}
			}
			finally
			{
				TestDb.ExecuteSql(table.DropTable());
			}
		}

		[TestMethod]
		public async Task InsertWithNulls()
		{
			var dataModel = _entitySchema;
			var table = _entitySchema.EntityTable;
			var database = new EntityDatabase<ObjectWithPocoSubModels>(_entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await database.Insert(modelInstance)
					.ExecuteAsync();

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(table.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.IsTrue(queryResult.IsDBNull(queryResult.GetOrdinal("ModelA_Data")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1_Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2_Data")));
				}
			}
			finally
			{
				TestDb.ExecuteSql(table.DropTable());
			}
		}

		[TestMethod]
		public async Task InsertViewOfObjectWithRequiredProperties()
		{
			var dataModel = _entitySchema;
			var table = _entitySchema.EntityTable;
			var database = new EntityDatabase<ObjectWithPocoSubModels>(_entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var modelInstance = new ViewOfObjectWithRequiredProperties
				{
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await database
					.Insert<ViewOfObjectWithRequiredProperties>(modelInstance)
					.ExecuteAsync();

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(table.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.IsTrue(queryResult.IsDBNull(queryResult.GetOrdinal("ModelA_Data")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1_Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2_Data")));
				}
			}
			finally
			{
				TestDb.ExecuteSql(table.DropTable());
			}
		}

		[TestMethod]
		public async Task InsertViewOfObjectWithRequiredPropertiesAsViews()
		{
			var dataModel = _entitySchema;
			var table = _entitySchema.EntityTable;
			var database = new EntityDatabase<ObjectWithPocoSubModels>(_entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var modelInstance = new ViewOfObjectWithRequiredPropertiesAsViews
				{
					ModelB1 = new SubModelBView { Data = 5 },
					ModelB2 = new SubModelBView { Data = 10 }
				};
				await database
					.Insert<ViewOfObjectWithRequiredPropertiesAsViews>(modelInstance)
					.ExecuteAsync();

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(table.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.IsTrue(queryResult.IsDBNull(queryResult.GetOrdinal("ModelA_Data")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1_Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2_Data")));
				}
			}
			finally
			{
				TestDb.ExecuteSql(table.DropTable());
			}
		}

		[TestMethod]
		public async Task InsertFlatView()
		{
			var dataModel = _entitySchema;
			var table = _entitySchema.EntityTable;
			var database = new EntityDatabase<ObjectWithPocoSubModels>(_entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var modelInstance = new ObjectWithPocoSubModelsView
				{
					ModelA_Data = "Hello World",
					ModelB1_Data = 5,
					ModelB2_Data = 10
				};
				await database
					.Insert<ObjectWithPocoSubModelsView>(modelInstance)
					.ExecuteAsync();

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(table.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.AreEqual(modelInstance.ModelA_Data, queryResult.GetString(queryResult.GetOrdinal("ModelA_Data")));
					Assert.AreEqual(modelInstance.ModelB1_Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1_Data")));
					Assert.AreEqual(modelInstance.ModelB2_Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2_Data")));
				}
			}
			finally
			{
				TestDb.ExecuteSql(table.DropTable());
			}
		}

		[TestMethod]
		public async Task UpdateWithoutNulls()
		{
			var dataModel = _entitySchema;
			var table = _entitySchema.EntityTable;
			var database = new EntityDatabase<ObjectWithPocoSubModels>(_entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await database.Insert(modelInstance)
					.ExecuteAsync();
				modelInstance.ModelA.Data = "Changed World";
				modelInstance.ModelB1.Data = 15;
				modelInstance.ModelB2.Data = 20;
				await database.Update(modelInstance)
					.ExecuteAsync();

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(table.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.AreEqual(modelInstance.ModelA.Data, queryResult.GetString(queryResult.GetOrdinal("ModelA_Data")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1_Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2_Data")));
				}
			}
			finally
			{
				TestDb.ExecuteSql(table.DropTable());
			}
		}

		[TestMethod]
		public async Task UpdateWithNulls()
		{
			var dataModel = _entitySchema;
			var table = _entitySchema.EntityTable;
			var database = new EntityDatabase<ObjectWithPocoSubModels>(_entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await database.Insert(modelInstance)
					.ExecuteAsync();
				modelInstance.ModelA = null;
				modelInstance.ModelB1.Data = 15;
				modelInstance.ModelB2.Data = 20;
				await database.Update(modelInstance)
					.ExecuteAsync();

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(table.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.IsTrue(queryResult.IsDBNull(queryResult.GetOrdinal("ModelA_Data")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1_Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2_Data")));
				}
			}
			finally
			{
				TestDb.ExecuteSql(table.DropTable());
			}
		}

		[TestMethod]
		public async Task UpdateViewOfObjectWithRequiredProperties()
		{
			var dataModel = _entitySchema;
			var table = _entitySchema.EntityTable;
			var database = new EntityDatabase<ObjectWithPocoSubModels>(_entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var modelInstance = new ViewOfObjectWithRequiredProperties
				{
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await database
					.Insert<ViewOfObjectWithRequiredProperties>(modelInstance)
					.ExecuteAsync();
				modelInstance.ModelB1.Data = 15;
				modelInstance.ModelB2.Data = 20;
				await database
					.Update<ViewOfObjectWithRequiredProperties>(modelInstance)
					.ExecuteAsync();

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(table.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.IsTrue(queryResult.IsDBNull(queryResult.GetOrdinal("ModelA_Data")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1_Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2_Data")));
				}
			}
			finally
			{
				TestDb.ExecuteSql(table.DropTable());
			}
		}

		[TestMethod]
		public async Task UpdateViewOfObjectWithRequiredPropertiesAsViews()
		{
			var dataModel = _entitySchema;
			var table = _entitySchema.EntityTable;
			var database = new EntityDatabase<ObjectWithPocoSubModels>(_entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var modelInstance = new ViewOfObjectWithRequiredPropertiesAsViews
				{
					ModelB1 = new SubModelBView { Data = 5 },
					ModelB2 = new SubModelBView { Data = 10 }
				};
				await database
					.Insert<ViewOfObjectWithRequiredPropertiesAsViews>(modelInstance)
					.ExecuteAsync();
				modelInstance.ModelB1.Data = 15;
				modelInstance.ModelB2.Data = 20;
				await database
					.Update<ViewOfObjectWithRequiredPropertiesAsViews>(modelInstance)
					.ExecuteAsync();

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(table.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.IsTrue(queryResult.IsDBNull(queryResult.GetOrdinal("ModelA_Data")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1_Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2_Data")));
				}
			}
			finally
			{
				TestDb.ExecuteSql(table.DropTable());
			}
		}

		[TestMethod]
		public async Task UpdateFlatView()
		{
			var dataModel = _entitySchema;
			var table = _entitySchema.EntityTable;
			var database = new EntityDatabase<ObjectWithPocoSubModels>(_entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var modelInstance = new ObjectWithPocoSubModelsView
				{
					ModelA_Data = "Hello World",
					ModelB1_Data = 5,
					ModelB2_Data = 10
				};
				await database
					.Insert<ObjectWithPocoSubModelsView>(modelInstance)
					.ExecuteAsync();
				modelInstance.ModelA_Data = "Changed World!";
				modelInstance.ModelB1_Data = 15;
				modelInstance.ModelB2_Data = 20;
				await database
					.Update<ObjectWithPocoSubModelsView>(modelInstance)
					.ExecuteAsync();

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(table.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.AreEqual(modelInstance.ModelA_Data, queryResult.GetString(queryResult.GetOrdinal("ModelA_Data")));
					Assert.AreEqual(modelInstance.ModelB1_Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1_Data")));
					Assert.AreEqual(modelInstance.ModelB2_Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2_Data")));
				}
			}
			finally
			{
				TestDb.ExecuteSql(table.DropTable());
			}
		}

		[TestMethod]
		public async Task Delete()
		{
			var dataModel = _entitySchema;
			var table = _entitySchema.EntityTable;
			var database = new EntityDatabase<ObjectWithPocoSubModels>(_entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await database.Insert(modelInstance)
					.ExecuteAsync();
				await database.Delete(modelInstance)
					.ExecuteAsync();

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(table.TableName)
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
		public async Task DeleteViewOfObjectWithRequiredProperties()
		{
			var dataModel = _entitySchema;
			var table = _entitySchema.EntityTable;
			var database = new EntityDatabase<ObjectWithPocoSubModels>(_entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var modelInstance = new ViewOfObjectWithRequiredProperties
				{
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await database
					.Insert<ViewOfObjectWithRequiredProperties>(modelInstance)
					.ExecuteAsync();
				await database
					.Delete<ViewOfObjectWithRequiredProperties>(modelInstance)
					.ExecuteAsync();

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(table.TableName)
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
		public async Task DeleteViewOfObjectWithRequiredPropertiesAsViews()
		{
			var dataModel = _entitySchema;
			var table = _entitySchema.EntityTable;
			var database = new EntityDatabase<ObjectWithPocoSubModels>(_entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var modelInstance = new ViewOfObjectWithRequiredPropertiesAsViews
				{
					ModelB1 = new SubModelBView { Data = 5 },
					ModelB2 = new SubModelBView { Data = 10 }
				};
				await database
					.Insert<ViewOfObjectWithRequiredPropertiesAsViews>(modelInstance)
					.ExecuteAsync();
				await database
					.Delete<ViewOfObjectWithRequiredPropertiesAsViews>(modelInstance)
					.ExecuteAsync();

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(table.TableName)
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
		public async Task DeleteFlatView()
		{
			var dataModel = _entitySchema;
			var table = _entitySchema.EntityTable;
			var database = new EntityDatabase<ObjectWithPocoSubModels>(_entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var modelInstance = new ObjectWithPocoSubModelsView
				{
					ModelA_Data = "Hello World",
					ModelB1_Data = 5,
					ModelB2_Data = 10
				};
				await database
					.Insert<ObjectWithPocoSubModelsView>(modelInstance)
					.ExecuteAsync();
				await database
					.Delete<ObjectWithPocoSubModelsView>(modelInstance)
					.ExecuteAsync();

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(table.TableName)
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
		public async Task SelectInflatedPocoWithConventions()
		{
			var dataModel = _entitySchema;
			var table = _entitySchema.EntityTable;
			var database = new EntityDatabase<ObjectWithPocoSubModels>(_entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await database.Insert(modelInstance)
					.ExecuteAsync();

				var selectResults = await database.Select<ObjectWithPocoSubModels>()
					.ExecuteAsync();

				Assert.AreEqual(1, selectResults.Count);
				var selectedInstance = selectResults.First();
				Assert.AreEqual(modelInstance.ModelA.Data, selectedInstance.ModelA.Data);
				Assert.AreEqual(modelInstance.ModelB1.Data, selectedInstance.ModelB1.Data);
				Assert.AreEqual(modelInstance.ModelB2.Data, selectedInstance.ModelB2.Data);
			}
			finally
			{
				TestDb.ExecuteSql(table.DropTable());
			}
		}

		[TestMethod]
		public async Task SelectInflatedPocoWithConventionsWithRequiredProperties()
		{
			var dataModel = _entitySchema;
			var table = _entitySchema.EntityTable;
			var database = new EntityDatabase<ObjectWithPocoSubModels>(_entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await database.Insert(modelInstance)
					.ExecuteAsync();

				var selectResults = await database
					.Select<ViewOfObjectWithRequiredProperties>()
					.ExecuteAsync();

				Assert.AreEqual(1, selectResults.Count);
				var selectedInstance = selectResults.First();
				Assert.AreEqual(modelInstance.ModelB1.Data, selectedInstance.ModelB1.Data);
				Assert.AreEqual(modelInstance.ModelB2.Data, selectedInstance.ModelB2.Data);
			}
			finally
			{
				TestDb.ExecuteSql(table.DropTable());
			}
		}

		[TestMethod]
		public async Task SelectInflatedPocoWithRequiredPropertiesAsViews()
		{
			var dataModel = _entitySchema;
			var table = _entitySchema.EntityTable;
			var database = new EntityDatabase<ObjectWithPocoSubModels>(_entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await database.Insert(modelInstance)
					.ExecuteAsync();

				var selectResults = await database
					.Select<ViewOfObjectWithRequiredPropertiesAsViews>()
					.ExecuteAsync();

				Assert.AreEqual(1, selectResults.Count);
				var selectedInstance = selectResults.First();
				Assert.AreEqual(modelInstance.ModelB1.Data, selectedInstance.ModelB1.Data);
				Assert.AreEqual(modelInstance.ModelB2.Data, selectedInstance.ModelB2.Data);
			}
			finally
			{
				TestDb.ExecuteSql(table.DropTable());
			}
		}

		[TestMethod]
		public async Task SelectFlatPocoWithConventions()
		{
			var dataModel = _entitySchema;
			var table = _entitySchema.EntityTable;
			var database = new EntityDatabase<ObjectWithPocoSubModels>(_entitySchema, TestDb.Provider);

			if (TestDb.ExecuteAndRead<int>(table.TableExists()) == 1)
				TestDb.ExecuteSql(table.DropTable());
			TestDb.ExecuteSql(table.CreateTable());

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await database.Insert(modelInstance)
					.ExecuteAsync();

				var selectResults = await database.Select<ObjectWithPocoSubModelsView>()
					.ExecuteAsync();

				Assert.AreEqual(1, selectResults.Count);
				var selectedInstance = selectResults.First();
				Assert.AreEqual(modelInstance.ModelA.Data, selectedInstance.ModelA_Data);
				Assert.AreEqual(modelInstance.ModelB1.Data, selectedInstance.ModelB1_Data);
				Assert.AreEqual(modelInstance.ModelB2.Data, selectedInstance.ModelB2_Data);
			}
			finally
			{
				TestDb.ExecuteSql(table.DropTable());
			}
		}

		private class ObjectWithPocoSubModels
		{
			public Guid Id { get; private set; }
			public SubModelA ModelA { get; set; }
			public SubModelB ModelB1 { get; set; }
			public SubModelB ModelB2 { get; set; }
		}

		private class ObjectWithPocoSubModelsView
		{
			public Guid Id { get; private set; }
			public string ModelA_Data { get; set; }
			public int ModelB1_Data { get; set; }
			public int ModelB2_Data { get; set; }
		}

		private class SubModelA
		{
			public string Data { get; set; }
		}

		private class SubModelB
		{
			public int Data { get; set; }
		}

		private class ViewOfObjectWithRequiredProperties
		{
			public Guid Id { get; private set; }
			public SubModelB ModelB1 { get; set; }
			public SubModelB ModelB2 { get; set; }
		}

		private class ViewOfObjectWithRequiredPropertiesAsViews
		{
			public Guid Id { get; private set; }
			public SubModelBView ModelB1 { get; set; }
			public SubModelBView ModelB2 { get; set; }
		}

		private class SubModelBView
		{
			public int Data { get; set; }
		}
	}
}
