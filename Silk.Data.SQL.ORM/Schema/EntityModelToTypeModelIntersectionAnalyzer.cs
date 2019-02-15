using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.Analysis.CandidateSources;
using Silk.Data.Modelling.Analysis.Rules;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	public class EntityModelToTypeModelIntersectionAnalyzer :
		IntersectAnalyzerBase<EntityModel, EntityField, TypeModel, PropertyInfoField>
	{
		public EntityModelToTypeModelIntersectionAnalyzer()
		{
			CandidateSources.Add(new ExactPathMatchCandidateSource<EntityModel, EntityField, TypeModel, PropertyInfoField>());
			CandidateSources.Add(new FlattenedNameMatchCandidateSource<EntityModel, EntityField, TypeModel, PropertyInfoField>());

			IntersectionRules.Add(new SameDataTypeRule<EntityModel, EntityField, TypeModel, PropertyInfoField>());
			IntersectionRules.Add(new BothNumericTypesRule<EntityModel, EntityField, TypeModel, PropertyInfoField>());
			IntersectionRules.Add(new ConvertableWithToStringRule<EntityModel, EntityField, TypeModel, PropertyInfoField>());
			IntersectionRules.Add(new ExplicitCastRule<EntityModel, EntityField, TypeModel, PropertyInfoField>());
			IntersectionRules.Add(new ConvertableWithTryParse<EntityModel, EntityField, TypeModel, PropertyInfoField>());

		}

		protected override IIntersection<EntityModel, EntityField, TypeModel, PropertyInfoField> CreateIntersection(
			IntersectAnalysis analysis
			)
			=> new Intersection(analysis.LeftModel, analysis.RightModel, analysis.IntersectedFields.ToArray());

		public class Intersection : IntersectionBase<EntityModel, EntityField, TypeModel, PropertyInfoField>
		{
			public Intersection(
				EntityModel leftModel, TypeModel rightModel,
				IntersectedFields<EntityModel, EntityField, TypeModel, PropertyInfoField>[] intersectedFields
				) : base(leftModel, rightModel, intersectedFields)
			{
			}
		}
	}
}
