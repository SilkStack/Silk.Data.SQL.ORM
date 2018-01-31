using Silk.Data.Modelling;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	/// <summary>
	/// Controls how schemas are designed.
	/// </summary>
	public interface ISchemaConvention
	{
		/// <summary>
		/// Visit a model.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="builder"></param>
		void VisitModel(TypedModel model, SchemaBuilder builder);
	}
}
