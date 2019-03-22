using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis.CandidateSources;
using Silk.Data.Modelling.Analysis.Rules;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Tests.Modelling
{
	[TestClass]
	public class ClassToEntityIntersectionAnalyzerTests
	{
		[TestMethod]
		public void CreateIntersection_WithTypeConverter_Returns_Mapped_Converted_Fields()
		{
			var schema = new SchemaBuilder()
				.Define<EntityModel>()
				.Build();
			var intersectionAnalyzer = new ClassToEntityIntersectionAnalyzer(
				new IIntersectCandidateSource<ViewIntersectionModel, ViewIntersectionField, ORM.Schema.EntityModel, EntityField>[] {
					new FlattenedNameMatchCandidateSource<ViewIntersectionModel, ViewIntersectionField, ORM.Schema.EntityModel, EntityField>()
				},
				new IIntersectionRule<ViewIntersectionModel, ViewIntersectionField, ORM.Schema.EntityModel, EntityField>[] {
					new TypeConverterRule<ViewIntersectionModel, ViewIntersectionField, ORM.Schema.EntityModel, EntityField>(
						new ITypeConverter[] { new SubConverter() }
						),
					new SameDataTypeRule<ViewIntersectionModel, ViewIntersectionField, ORM.Schema.EntityModel, EntityField>()
				}
				);
			var intersection = intersectionAnalyzer.CreateIntersection(
				TypeModel.GetModelOf<ViewModel>(), schema.GetEntityModel<EntityModel>()
				);

			Assert.IsNotNull(intersection);
			Assert.AreEqual(1, intersection.IntersectedFields.Length);
			Assert.IsInstanceOfType(
				intersection.IntersectedFields[0].LeftPath.Fields[0],
				typeof(ConvertedViewIntersectionField<ViewModelSub, EntityModelSub>)
				);
		}

		private class EntityModel
		{
			public EntityModelSub Sub { get; set; }
		}

		private class EntityModelSub
		{
			public string Data { get; set; }
		}

		private class ViewModel
		{
			public ViewModelSub Sub { get; set; }
		}

		private class ViewModelSub
		{
			public string CustomData { get; set; }
		}

		private class SubConverter : TypeConverter<ViewModelSub, EntityModelSub>
		{
			public override bool TryConvert(ViewModelSub from, out EntityModelSub to)
			{
				to = new EntityModelSub { Data = from.CustomData };
				return true;
			}
		}
	}
}
