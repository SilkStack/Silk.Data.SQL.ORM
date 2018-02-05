using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public class ManyToOneConvention : ViewConvention<DataViewBuilder>
	{
		public override ViewType SupportedViewTypes => ViewType.ConventionDerived;
		public override bool PerformMultiplePasses => true;
		public override bool SkipIfFieldDefined => false;

		public override void MakeModelField(DataViewBuilder viewBuilder, ModelField field)
		{
			MakeConventionDrivenFields(viewBuilder, field);
		}

		public void MakeConventionDrivenFields(DataViewBuilder viewBuilder, ModelField field)
		{
			//if (viewBuilder.IsPrimitiveType(field.DataType) || field.IsEnumerable)
			//	return;

			//if (viewBuilder.IsFieldDefined(field.Name))
			//	return;

			//var dataTypeSchemaDefinition = viewBuilder.GetSchemaDefinitionFor(field.DataType);
			//if (dataTypeSchemaDefinition == null)
			//	return;

			//var dataTypeEntityTable = dataTypeSchemaDefinition.TableDefinitions.FirstOrDefault(q => q.IsEntityTable);
			//if (dataTypeEntityTable == null)
			//	return;

			//var dataTypePrimaryKeyFields = dataTypeEntityTable.Fields.Where(q => q.Metadata.OfType<PrimaryKeyAttribute>().Any()).ToArray();
			//if (dataTypePrimaryKeyFields.Length == 0)
			//	return;

			//var sourceField = viewBuilder.FindSourceField(field, field.Name);
			//if (sourceField == null)
			//	return;

			//var relationshipDefinition = new RelationshipDefinition
			//{
			//	EntityType = sourceField.Field.DataType,
			//	RelationshipType = RelationshipType.ManyToOne
			//};

			//var tableReference = relationshipDefinition.CreateTableReference();
			//foreach (var primaryKeyField in dataTypePrimaryKeyFields)
			//{
			//	tableReference.AddReferenceToEntityTable(sourceField, dataTypeEntityTable, primaryKeyField);
			//}

			//viewBuilder.DefineManyToOneViewField(sourceField, new[] { sourceField.Field.Name }, field.Name,
			//	relationshipDefinition,
			//	new IsNullableAttribute(true));
		}
	}
}
