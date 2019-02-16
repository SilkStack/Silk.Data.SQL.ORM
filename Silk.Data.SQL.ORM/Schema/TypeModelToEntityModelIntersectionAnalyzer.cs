using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Computes the intersection of a type model with an entity model.
	/// Used for reading from an entity type into an INSERT or UPDATE query, or for expression analysis.
	/// </summary>
	public class TypeModelToEntityModelIntersectionAnalyzer :
		DefaultIntersectionAnalyzer<TypeModel, PropertyInfoField, EntityModel, EntityField>
	{
	}
}
