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
				if (builder.IsFieldDefined(model, field.Name))
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
			foreach (var subField in dataTypeModel.Fields)
			{
				var fieldName = $"{field.Name}_{subField.Name}";
				if (builder.IsFieldDefined(model, fieldName))
					continue;

				//  I need to visit the other schema conventions using this field here somehow
				//  so that embedding a full tree, or using relationships etc. all follows the same
				//  rules as top-level fields
			}
		}
    }
}
