using Silk.Data.Modelling;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	/// <summary>
	/// Controls how projections are built.
	/// </summary>
	public interface IProjectionConvention
	{
		/// <summary>
		/// Visit a model field to possibly create projected fields.
		/// </summary>
		/// <param name="modelField"></param>
		/// <param name="schemaBuilder"></param>
		void VisitField(ModelField modelField, ProjectionBuilder builder);
	}
}
