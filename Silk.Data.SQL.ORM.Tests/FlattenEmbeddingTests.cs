using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class FlattenEmbeddingTests
	{
		private static EntityModel<ObjectWithPocoSubModels> _conventionModel =
			TestDb.CreateDomainAndModel<ObjectWithPocoSubModels>();
		private static EntityModel<ObjectWithPocoSubModels, ObjectWithPocoSubModelsView> _viewModelModel =
			TestDb.CreateDomainAndModel<ObjectWithPocoSubModels, ObjectWithPocoSubModelsView>();

		[TestMethod]
		public void FlattenPocoInDataModelWithConventions()
		{
			var dataModel = _conventionModel;

			Assert.AreEqual(4, dataModel.Fields.Length);
			Assert.IsTrue(dataModel.Fields.Any(
				q => q.Name == "Id" && q.DataType == typeof(Guid) &&
					q.ModelBinding.ModelFieldPath.SequenceEqual(new[] { "Id" })
				));
			Assert.IsTrue(dataModel.Fields.Any(
				q => q.Name == "ModelAData" && q.DataType == typeof(string) &&
					q.ModelBinding.ModelFieldPath.SequenceEqual(new[] { "ModelA", "Data" })
				));
			Assert.IsTrue(dataModel.Fields.Any(
				q => q.Name == "ModelB1Data" && q.DataType == typeof(int) &&
					q.ModelBinding.ModelFieldPath.SequenceEqual(new[] { "ModelB1", "Data" })
				));
			Assert.IsTrue(dataModel.Fields.Any(
				q => q.Name == "ModelB2Data" && q.DataType == typeof(int) &&
					q.ModelBinding.ModelFieldPath.SequenceEqual(new[] { "ModelB2", "Data" })
				));
		}

		[TestMethod]
		public async Task InsertConventionModelWithoutNulls()
		{
			var dataModel = _conventionModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.AreEqual(modelInstance.ModelA.Data, queryResult.GetString(queryResult.GetOrdinal("ModelAData")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2Data")));
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
		public async Task InsertConventionModelWithNulls()
		{
			var dataModel = _conventionModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.IsTrue(queryResult.IsDBNull(queryResult.GetOrdinal("ModelAData")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2Data")));
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
		public async Task InsertViewOfObjectWithRequiredProperties()
		{
			var dataModel = _conventionModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ViewOfObjectWithRequiredProperties
				{
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.IsTrue(queryResult.IsDBNull(queryResult.GetOrdinal("ModelAData")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2Data")));
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
		public async Task InsertViewOfObjectWithRequiredPropertiesAsViews()
		{
			var dataModel = _conventionModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ViewOfObjectWithRequiredPropertiesAsViews
				{
					ModelB1 = new SubModelBView { Data = 5 },
					ModelB2 = new SubModelBView { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.IsTrue(queryResult.IsDBNull(queryResult.GetOrdinal("ModelAData")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2Data")));
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
		public async Task InsertFlatViewOfConventionModel()
		{
			var dataModel = _conventionModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModelsView
				{
					ModelAData = "Hello World",
					ModelB1Data = 5,
					ModelB2Data = 10
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.AreEqual(modelInstance.ModelAData, queryResult.GetString(queryResult.GetOrdinal("ModelAData")));
					Assert.AreEqual(modelInstance.ModelB1Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1Data")));
					Assert.AreEqual(modelInstance.ModelB2Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2Data")));
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
		public async Task UpdateConventionModelWithoutNulls()
		{
			var dataModel = _conventionModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);
				modelInstance.ModelA.Data = "Changed World";
				modelInstance.ModelB1.Data = 15;
				modelInstance.ModelB2.Data = 20;
				await dataModel.Update(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.AreEqual(modelInstance.ModelA.Data, queryResult.GetString(queryResult.GetOrdinal("ModelAData")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2Data")));
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
		public async Task UpdateConventionModelWithNulls()
		{
			var dataModel = _conventionModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);
				modelInstance.ModelA = null;
				modelInstance.ModelB1.Data = 15;
				modelInstance.ModelB2.Data = 20;
				await dataModel.Update(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.IsTrue(queryResult.IsDBNull(queryResult.GetOrdinal("ModelAData")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2Data")));
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
		public async Task UpdateViewOfObjectWithRequiredProperties()
		{
			var dataModel = _conventionModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ViewOfObjectWithRequiredProperties
				{
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);
				modelInstance.ModelB1.Data = 15;
				modelInstance.ModelB2.Data = 20;
				await dataModel.Update(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.IsTrue(queryResult.IsDBNull(queryResult.GetOrdinal("ModelAData")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2Data")));
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
		public async Task UpdateViewOfObjectWithRequiredPropertiesAsViews()
		{
			var dataModel = _conventionModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ViewOfObjectWithRequiredPropertiesAsViews
				{
					ModelB1 = new SubModelBView { Data = 5 },
					ModelB2 = new SubModelBView { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);
				modelInstance.ModelB1.Data = 15;
				modelInstance.ModelB2.Data = 20;
				await dataModel.Update(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.IsTrue(queryResult.IsDBNull(queryResult.GetOrdinal("ModelAData")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2Data")));
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
		public async Task UpdateFlatViewOfConventionModel()
		{
			var dataModel = _conventionModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModelsView
				{
					ModelAData = "Hello World",
					ModelB1Data = 5,
					ModelB2Data = 10
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);
				modelInstance.ModelAData = "Changed World!";
				modelInstance.ModelB1Data = 15;
				modelInstance.ModelB2Data = 20;
				await dataModel.Update(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.AreEqual(modelInstance.ModelAData, queryResult.GetString(queryResult.GetOrdinal("ModelAData")));
					Assert.AreEqual(modelInstance.ModelB1Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1Data")));
					Assert.AreEqual(modelInstance.ModelB2Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2Data")));
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
		public async Task DeleteConventionModel()
		{
			var dataModel = _conventionModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);
				await dataModel.Delete(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
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
		public async Task DeleteViewOfObjectWithRequiredProperties()
		{
			var dataModel = _conventionModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ViewOfObjectWithRequiredProperties
				{
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);
				await dataModel.Delete(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
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
		public async Task DeleteViewOfObjectWithRequiredPropertiesAsViews()
		{
			var dataModel = _conventionModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ViewOfObjectWithRequiredPropertiesAsViews
				{
					ModelB1 = new SubModelBView { Data = 5 },
					ModelB2 = new SubModelBView { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);
				await dataModel.Delete(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
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
		public async Task DeleteFlatViewOfConventionModel()
		{
			var dataModel = _conventionModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModelsView
				{
					ModelAData = "Hello World",
					ModelB1Data = 5,
					ModelB2Data = 10
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);
				await dataModel.Delete(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
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
		public async Task SelectInflatedPocoWithConventions()
		{
			var dataModel = _conventionModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				var selectResults = await dataModel.Select()
					.ExecuteAsync(TestDb.Provider);

				Assert.AreEqual(1, selectResults.Count);
				var selectedInstance = selectResults.First();
				Assert.AreEqual(modelInstance.ModelA.Data, selectedInstance.ModelA.Data);
				Assert.AreEqual(modelInstance.ModelB1.Data, selectedInstance.ModelB1.Data);
				Assert.AreEqual(modelInstance.ModelB2.Data, selectedInstance.ModelB2.Data);
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
		public async Task SelectInflatedPocoWithConventionsWithRequiredProperties()
		{
			var dataModel = _conventionModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				var selectResults = await dataModel.Select<ViewOfObjectWithRequiredProperties>()
					.ExecuteAsync(TestDb.Provider);

				Assert.AreEqual(1, selectResults.Count);
				var selectedInstance = selectResults.First();
				Assert.AreEqual(modelInstance.ModelB1.Data, selectedInstance.ModelB1.Data);
				Assert.AreEqual(modelInstance.ModelB2.Data, selectedInstance.ModelB2.Data);
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
		public async Task SelectInflatedPocoWithConventionsWithRequiredPropertiesAsViews()
		{
			var dataModel = _conventionModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				var selectResults = await dataModel.Select<ViewOfObjectWithRequiredPropertiesAsViews>()
					.ExecuteAsync(TestDb.Provider);

				Assert.AreEqual(1, selectResults.Count);
				var selectedInstance = selectResults.First();
				Assert.AreEqual(modelInstance.ModelB1.Data, selectedInstance.ModelB1.Data);
				Assert.AreEqual(modelInstance.ModelB2.Data, selectedInstance.ModelB2.Data);
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
		public async Task SelectFlatPocoWithConventions()
		{
			var dataModel = _conventionModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				var selectResults = await dataModel.Select<ObjectWithPocoSubModelsView>()
					.ExecuteAsync(TestDb.Provider);

				Assert.AreEqual(1, selectResults.Count);
				var selectedInstance = selectResults.First();
				Assert.AreEqual(modelInstance.ModelA.Data, selectedInstance.ModelAData);
				Assert.AreEqual(modelInstance.ModelB1.Data, selectedInstance.ModelB1Data);
				Assert.AreEqual(modelInstance.ModelB2.Data, selectedInstance.ModelB2Data);
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
		public void FlattenPocoInDataModelWithViewModel()
		{
			var dataModel = _viewModelModel;

			Assert.AreEqual(4, dataModel.Fields.Length);
			Assert.IsTrue(dataModel.Fields.Any(
				q => q.Name == "Id" && q.DataType == typeof(Guid) &&
					q.ModelBinding.ModelFieldPath.SequenceEqual(new[] { "Id" })
				));
			Assert.IsTrue(dataModel.Fields.Any(
				q => q.Name == "ModelAData" && q.DataType == typeof(string) &&
					q.ModelBinding.ModelFieldPath.SequenceEqual(new[] { "ModelA", "Data" })
				));
			Assert.IsTrue(dataModel.Fields.Any(
				q => q.Name == "ModelB1Data" && q.DataType == typeof(int) &&
					q.ModelBinding.ModelFieldPath.SequenceEqual(new[] { "ModelB1", "Data" })
				));
			Assert.IsTrue(dataModel.Fields.Any(
				q => q.Name == "ModelB2Data" && q.DataType == typeof(int) &&
					q.ModelBinding.ModelFieldPath.SequenceEqual(new[] { "ModelB2", "Data" })
				));
		}

		[TestMethod]
		public async Task InsertViewModelModelWithoutNulls()
		{
			var dataModel = _viewModelModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.AreEqual(modelInstance.ModelA.Data, queryResult.GetString(queryResult.GetOrdinal("ModelAData")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2Data")));
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
		public async Task InsertViewModelModelWithNulls()
		{
			var dataModel = _viewModelModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.IsTrue(queryResult.IsDBNull(queryResult.GetOrdinal("ModelAData")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2Data")));
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
		public async Task InsertFlatViewOfViewModelModel()
		{
			var dataModel = _viewModelModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModelsView
				{
					ModelAData = "Hello World",
					ModelB1Data = 5,
					ModelB2Data = 10
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.AreEqual(modelInstance.ModelAData, queryResult.GetString(queryResult.GetOrdinal("ModelAData")));
					Assert.AreEqual(modelInstance.ModelB1Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1Data")));
					Assert.AreEqual(modelInstance.ModelB2Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2Data")));
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
		public async Task UpdateViewModelModelWithoutNulls()
		{
			var dataModel = _viewModelModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);
				modelInstance.ModelA.Data = "Changed World";
				modelInstance.ModelB1.Data = 15;
				modelInstance.ModelB2.Data = 20;
				await dataModel.Update(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.AreEqual(modelInstance.ModelA.Data, queryResult.GetString(queryResult.GetOrdinal("ModelAData")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2Data")));
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
		public async Task UpdateViewModelModelWithNulls()
		{
			var dataModel = _viewModelModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);
				modelInstance.ModelA = null;
				modelInstance.ModelB1.Data = 15;
				modelInstance.ModelB2.Data = 20;
				await dataModel.Update(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.IsTrue(queryResult.IsDBNull(queryResult.GetOrdinal("ModelAData")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2Data")));
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
		public async Task UpdateViewModelModelOfObjectWithRequiredProperties()
		{
			var dataModel = _viewModelModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ViewOfObjectWithRequiredProperties
				{
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);
				modelInstance.ModelB1.Data = 15;
				modelInstance.ModelB2.Data = 20;
				await dataModel.Update(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.IsTrue(queryResult.IsDBNull(queryResult.GetOrdinal("ModelAData")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2Data")));
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
		public async Task UpdateViewModelModelOfObjectWithRequiredPropertiesAsViews()
		{
			var dataModel = _viewModelModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ViewOfObjectWithRequiredPropertiesAsViews
				{
					ModelB1 = new SubModelBView { Data = 5 },
					ModelB2 = new SubModelBView { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);
				modelInstance.ModelB1.Data = 15;
				modelInstance.ModelB2.Data = 20;
				await dataModel.Update(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.IsTrue(queryResult.IsDBNull(queryResult.GetOrdinal("ModelAData")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2Data")));
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
		public async Task UpdateFlatViewOfViewModelModel()
		{
			var dataModel = _viewModelModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModelsView
				{
					ModelAData = "Hello World",
					ModelB1Data = 5,
					ModelB2Data = 10
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);
				modelInstance.ModelAData = "Changed World!";
				modelInstance.ModelB1Data = 15;
				modelInstance.ModelB2Data = 20;
				await dataModel.Update(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(modelInstance.Id, queryResult.GetGuid(queryResult.GetOrdinal(nameof(modelInstance.Id))));
					Assert.AreEqual(modelInstance.ModelAData, queryResult.GetString(queryResult.GetOrdinal("ModelAData")));
					Assert.AreEqual(modelInstance.ModelB1Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1Data")));
					Assert.AreEqual(modelInstance.ModelB2Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2Data")));
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
		public async Task DeleteViewModelModel()
		{
			var dataModel = _viewModelModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);
				await dataModel.Delete(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
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
		public async Task DeleteViewModelModelOfObjectWithRequiredProperties()
		{
			var dataModel = _viewModelModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ViewOfObjectWithRequiredProperties
				{
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);
				await dataModel.Delete(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
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
		public async Task DeleteViewModelModelOfObjectWithRequiredPropertiesAsViews()
		{
			var dataModel = _viewModelModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ViewOfObjectWithRequiredPropertiesAsViews
				{
					ModelB1 = new SubModelBView { Data = 5 },
					ModelB2 = new SubModelBView { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);
				await dataModel.Delete(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
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
		public async Task DeleteFlatViewOfViewModelModel()
		{
			var dataModel = _viewModelModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModelsView
				{
					ModelAData = "Hello World",
					ModelB1Data = 5,
					ModelB2Data = 10
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);
				await dataModel.Delete(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				using (var queryResult = await TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table(dataModel.Schema.EntityTable.TableName)
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
		public async Task SelectInflatedPocoWithViewModelModel()
		{
			var dataModel = _viewModelModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				var selectResults = await dataModel.Select()
					.ExecuteAsync(TestDb.Provider);

				Assert.AreEqual(1, selectResults.Count);
				var selectedInstance = selectResults.First();
				Assert.AreEqual(modelInstance.ModelA.Data, selectedInstance.ModelA.Data);
				Assert.AreEqual(modelInstance.ModelB1.Data, selectedInstance.ModelB1.Data);
				Assert.AreEqual(modelInstance.ModelB2.Data, selectedInstance.ModelB2.Data);
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
		public async Task SelectInflatedPocoWithViewModelModelWithRequiredProperties()
		{
			var dataModel = _viewModelModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				var selectResults = await dataModel.Select<ViewOfObjectWithRequiredProperties>()
					.ExecuteAsync(TestDb.Provider);

				Assert.AreEqual(1, selectResults.Count);
				var selectedInstance = selectResults.First();
				Assert.AreEqual(modelInstance.ModelB1.Data, selectedInstance.ModelB1.Data);
				Assert.AreEqual(modelInstance.ModelB2.Data, selectedInstance.ModelB2.Data);
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
		public async Task SelectInflatedPocoWithViewModelModelWithRequiredPropertiesAsViews()
		{
			var dataModel = _viewModelModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				var selectResults = await dataModel.Select<ViewOfObjectWithRequiredPropertiesAsViews>()
					.ExecuteAsync(TestDb.Provider);

				Assert.AreEqual(1, selectResults.Count);
				var selectedInstance = selectResults.First();
				Assert.AreEqual(modelInstance.ModelB1.Data, selectedInstance.ModelB1.Data);
				Assert.AreEqual(modelInstance.ModelB2.Data, selectedInstance.ModelB2.Data);
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
		public async Task SelectFlatPocoWithViewModelModel()
		{
			var dataModel = _viewModelModel;

			foreach (var table in dataModel.Schema.Tables)
			{
				await table.CreateAsync(TestDb.Provider);
			}

			try
			{
				var modelInstance = new ObjectWithPocoSubModels
				{
					ModelA = new SubModelA { Data = "Hello World" },
					ModelB1 = new SubModelB { Data = 5 },
					ModelB2 = new SubModelB { Data = 10 }
				};
				await dataModel.Insert(modelInstance)
					.ExecuteAsync(TestDb.Provider);

				var selectResults = await dataModel.Select<ObjectWithPocoSubModelsView>()
					.ExecuteAsync(TestDb.Provider);

				Assert.AreEqual(1, selectResults.Count);
				var selectedInstance = selectResults.First();
				Assert.AreEqual(modelInstance.ModelA.Data, selectedInstance.ModelAData);
				Assert.AreEqual(modelInstance.ModelB1.Data, selectedInstance.ModelB1Data);
				Assert.AreEqual(modelInstance.ModelB2.Data, selectedInstance.ModelB2Data);
			}
			finally
			{
				foreach (var table in dataModel.Schema.Tables)
				{
					await table.DropAsync(TestDb.Provider);
				}
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
			public string ModelAData { get; set; }
			public int ModelB1Data { get; set; }
			public int ModelB2Data { get; set; }
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
