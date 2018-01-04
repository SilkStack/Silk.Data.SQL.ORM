using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using Silk.Data.Modelling.Conventions;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public class ProjectSubObjectProperties : ViewConvention<DataViewBuilder>
	{
		public override ViewType SupportedViewTypes => ViewType.ModelDriven;
		public override bool PerformMultiplePasses => false;
		public override bool SkipIfFieldDefined => true;

		public override void MakeModelField(DataViewBuilder viewBuilder, ModelField field)
		{
			if (viewBuilder.IsPrimitiveType(field.DataType) || field.IsEnumerable)
				return;

			var dataTypeSchemaDefinition = viewBuilder.GetSchemaDefinitionFor(field.DataType);
			if (dataTypeSchemaDefinition == null)
				return;

			var dataTypeEntityTable = dataTypeSchemaDefinition.TableDefinitions.FirstOrDefault(q => q.IsEntityTable);
			if (dataTypeEntityTable == null)
				return;

			var dataTypePrimaryKeyFields = dataTypeEntityTable.Fields.Where(q => q.Metadata.OfType<PrimaryKeyAttribute>().Any()).ToArray();
			if (dataTypePrimaryKeyFields.Length == 0)
				return;

			var sourceField = viewBuilder.FindSourceField(field, field.Name);
			if (sourceField == null)
				return;

			var relationshipDefinition = new RelationshipDefinition
			{
				EntityType = field.DataType,
				ProjectionType = sourceField.Field.DataType,
				RelationshipType = RelationshipType.ManyToOne
			};

			foreach (var primaryKeyField in dataTypePrimaryKeyFields)
			{
				viewBuilder.DefineAssignedViewField($"{sourceField.Field.Name}{primaryKeyField.Name}", primaryKeyField.DataType, BindingDirection.ModelToView,
					new[] { sourceField.Field.Name, primaryKeyField.Name },
					new IsNullableAttribute(true));
			}

			viewBuilder.DefineManyToOneViewField(sourceField, new[] { sourceField.Field.Name }, field.Name,
				relationshipDefinition,
				new IsNullableAttribute(true));
		}
	}
}
