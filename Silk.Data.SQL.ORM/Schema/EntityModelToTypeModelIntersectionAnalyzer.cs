using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.Analysis.CandidateSources;
using Silk.Data.Modelling.Analysis.Rules;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Computes the intersection of an entity model and type model.
	/// Used for mapping result data from SELECT queries.
	/// </summary>
	public class EntityModelToTypeModelIntersectionAnalyzer :
		DefaultIntersectionAnalyzer<EntityModel, EntityField, TypeModel, PropertyInfoField>
	{
	}
}
