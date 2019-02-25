using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;

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
