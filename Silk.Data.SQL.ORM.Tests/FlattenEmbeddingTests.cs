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
					Assert.AreEqual(modelInstance.ModelA.Data, queryResult.GetString(queryResult.GetOrdinal("ModelA_Data")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1_Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2_Data")));
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
					Assert.AreEqual(modelInstance.ModelA.Data, queryResult.GetString(queryResult.GetOrdinal("ModelA_Data")));
					Assert.AreEqual(modelInstance.ModelB1.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB1_Data")));
					Assert.AreEqual(modelInstance.ModelB2.Data, queryResult.GetInt32(queryResult.GetOrdinal("ModelB2_Data")));
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
	}
}
