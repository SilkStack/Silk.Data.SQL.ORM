using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public class FlattenPocosConvention : ISchemaConvention
	{
		public void VisitModel(TypedModel model, SchemaBuilder builder)
		{
			foreach (var field in model.Fields)
			{
				//  if the field has already been modelled by a previous convention we can skip it
				if (builder.IsFieldDefinedInContext(field.Name))
					continue;
				//  if the type is an entity type it needs a relationship modelled using another convention
				if (builder.IsEntityType(field.DataType))
					continue;

				FlattenIntoSchema(model, field, builder);
			}
		}

		private void FlattenIntoSchema(TypedModel model, ModelField field, SchemaBuilder builder)
		{
			var dataTypeModel = field.DataTypeModel;
			builder.PushModelOntoContext(dataTypeModel, field.Name);
			try
			{
				foreach (var schemaConvention in builder.Conventions)
				{
					schemaConvention.VisitModel(dataTypeModel, builder);
				}
			}
			finally
			{
				builder.PopModelOffContext();
			}
		}
	}
}
