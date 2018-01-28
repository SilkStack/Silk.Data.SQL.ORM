using Silk.Data.Modelling;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	/// <summary>
	/// Controls how schemas are designed.
	/// </summary>
	public interface ISchemaConvention
	{
		/// <summary>
		/// Visit a model field to possibly create fields on the schema.
		/// </summary>
		/// <param name="modelField"></param>
		/// <param name="schemaBuilder"></param>
		void VisitField(ModelField modelField, SchemaBuilder builder);
	}
}
