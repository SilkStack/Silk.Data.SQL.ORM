using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class FlattenEmbeddingTests
	{
		[TestMethod]
		public void FlattenPocoInDataModelWithConventions()
		{
			var dataModel = TestDb.CreateDomainAndModel<ObjectWithPocoSubModels>();

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
		public void FlattenPocoInDataModelWithViewModel()
		{
			var dataModel = TestDb.CreateDomainAndModel<ObjectWithPocoSubModels, ObjectWithPocoSubModelsView>();

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
