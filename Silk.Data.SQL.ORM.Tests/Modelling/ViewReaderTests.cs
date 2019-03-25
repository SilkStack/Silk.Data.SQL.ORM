using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Schema;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests.Modelling
{
	[TestClass]
	public class ViewReaderTests
	{
		[TestMethod]
		public void Read_NonConverted_Object_Returns_Correct_Value()
		{
			var schema = new SchemaBuilder()
				.Define<EntityModel>()
				.AddTypeConverters(new[] { new SubConverter() })
				.Build();
			var entityModel = schema.GetEntityModel<EntityModel>();
			var viewModel = entityModel.GetEntityView<ViewModel>();
			var instance = new ViewModel
			{
				Id = 1,
				Sub = new SubViewModel
				{
					OtherData = "Hello World"
				}
			};
			var reader = new ViewReader<ViewModel>(instance);
			var readPath = viewModel.ClassToEntityIntersection
				.IntersectedFields.First(q => q.LeftField.FieldName == "Id").LeftPath;
			var value = reader.Read<object>(readPath);
			Assert.AreEqual(instance.Id, value);
		}

		[TestMethod]
		public void Read_Converted_Object_Returns_Correct_Value()
		{
			var schema = new SchemaBuilder()
				.Define<EntityModel>()
				.AddTypeConverters(new[] { new SubConverter() })
				.Build();
			var entityModel = schema.GetEntityModel<EntityModel>();
			var viewModel = entityModel.GetEntityView<ViewModel>();
			var instance = new ViewModel
			{
				Id = 1,
				Sub = new SubViewModel
				{
					OtherData = "Hello World"
				}
			};
			var reader = new ViewReader<ViewModel>(instance);
			var readPath = viewModel.ClassToEntityIntersection
				.IntersectedFields.First(q => q.LeftField.FieldName == "Data").LeftPath;
			var value = reader.Read<object>(readPath);
			Assert.AreEqual(instance.Sub.OtherData, value);
		}

		private class EntityModel
		{
			public int Id { get; set; }
			public SubEntityModel Sub { get; set; }
		}

		private class SubEntityModel
		{
			public string Data { get; set; }
		}

		private class ViewModel
		{
			public int Id { get; set; }
			public SubViewModel Sub { get; set; }
		}

		private class SubViewModel
		{
			public string OtherData { get; set; }
		}

		private class SubConverter : TypeConverter<SubViewModel, SubEntityModel>
		{
			public override bool TryConvert(SubViewModel from, out SubEntityModel to)
			{
				to = new SubEntityModel { Data = from.OtherData };
				return true;
			}
		}
	}
}
