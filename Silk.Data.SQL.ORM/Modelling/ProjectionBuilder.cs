using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Modelling.Conventions;

namespace Silk.Data.SQL.ORM.Modelling
{
	/// <summary>
	/// API used in <see cref="IProjectionConvention">IProjectionConvention</see> to build projections.
	/// </summary>
	public class ProjectionBuilder
	{
		/// <summary>
		/// Gets the model of the entity being projected.
		/// </summary>
		public TypedModel EntityModel { get; }

		/// <summary>
		/// Gets the model of the desired projection.
		/// </summary>
		public Model ProjectionModel { get; }
	}
}
