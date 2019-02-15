using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Computes the intersection of a type model with an entity model.
	/// </summary>
	public class TypeModelToEntityModelIntersectionAnalyzer :
		IntersectAnalyzerBase<TypeModel, PropertyInfoField, EntityModel, EntityField>
	{
		protected override IIntersection<TypeModel, PropertyInfoField, EntityModel, EntityField> CreateIntersection(
			IntersectAnalysis analysis
			)
			=> new Intersection(analysis.LeftModel, analysis.RightModel, analysis.IntersectedFields.ToArray());

		public class Intersection : IntersectionBase<TypeModel, PropertyInfoField, EntityModel, EntityField>
		{
			public Intersection(
				TypeModel leftModel, EntityModel rightModel,
				IntersectedFields<TypeModel, PropertyInfoField, EntityModel, EntityField>[] intersectedFields
				) : base(leftModel, rightModel, intersectedFields)
			{
			}
		}
	}
}
