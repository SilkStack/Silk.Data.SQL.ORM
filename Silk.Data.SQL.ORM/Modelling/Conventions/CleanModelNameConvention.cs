using Silk.Data.Modelling;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public class CleanModelNameConvention : ISchemaConvention
	{
		public void VisitModel(TypedModel model, SchemaBuilder builder)
		{
			var entityDefinition = builder.GetEntityDefinition(model.DataType);
			if (entityDefinition == null)
				return;

			if (entityDefinition.TableName.EndsWith("DomainModel"))
				entityDefinition.TableName = entityDefinition.TableName.Substring(0, entityDefinition.TableName.Length - 11);
			if (entityDefinition.TableName.EndsWith("Model"))
				entityDefinition.TableName = entityDefinition.TableName.Substring(0, entityDefinition.TableName.Length - 5);
		}
	}
}
