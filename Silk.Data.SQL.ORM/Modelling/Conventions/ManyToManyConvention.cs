using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public class ManyToManyConvention : ViewConvention<DataViewBuilder>
	{
		public override ViewType SupportedViewTypes => ViewType.ConventionDerived;
		public override bool PerformMultiplePasses => true;
		public override bool SkipIfFieldDefined => true;

		public override void MakeModelField(DataViewBuilder viewBuilder, ModelField field)
		{
			MakeConventionDrivenFields(viewBuilder, field);
		}

		public void MakeConventionDrivenFields(DataViewBuilder viewBuilder, ModelField field)
		{
			if (!field.IsEnumerable)
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

			var thisPrimaryKeysFields = viewBuilder.ViewDefinition.FieldDefinitions.Where(q => q.Metadata.OfType<PrimaryKeyAttribute>().Any()).ToArray();
			if (thisPrimaryKeysFields.Length == 0)
				return;

			var sourceField = viewBuilder.FindSourceField(field, field.Name, dataType: field.DataType);
			if (sourceField == null)
				return;

			var thisTableDefinition = viewBuilder.GetSchemaDefinition().TableDefinitions.First(q => q.IsEntityTable);

			var sortedNames = new[] { thisTableDefinition.TableName, dataTypeEntityTable.TableName }
				.OrderBy(q => q).ToArray();
			var relationshipTableName = $"{sortedNames[0]}To{sortedNames[1]}";

			var relationshipTableDefinition = viewBuilder.GetSchemaDefinition().TableDefinitions.FirstOrDefault(q => q.TableName == relationshipTableName);
			if (relationshipTableDefinition == null)
			{
				relationshipTableDefinition = new TableDefinition
				{
					IsEntityTable = false,
					TableName = relationshipTableName
				};
				if (!viewBuilder.DomainDefinition.IsReadOnly)
					viewBuilder.GetSchemaDefinition().TableDefinitions.Add(relationshipTableDefinition);
			}

			viewBuilder.DefineAssignedViewField(sourceField, metadata: sourceField.Field.Metadata);
			var viewField = viewBuilder.GetDefinedField(sourceField.Field.Name);
			viewField.Metadata.Add(new RelationshipDefinition
			{
				EntityType = field.DataType,
				RelationshipType = RelationshipType.ManyToMany
			});

			foreach (var primaryKeyField in thisPrimaryKeysFields)
			{
				relationshipTableDefinition.Fields.Add(
					new ViewFieldDefinition($"{thisTableDefinition.TableName}_{primaryKeyField.Name}", null)
					{
						DataType = primaryKeyField.DataType
					});
			}

			foreach (var primaryKeyField in dataTypePrimaryKeyFields)
			{
				relationshipTableDefinition.Fields.Add(
					new ViewFieldDefinition($"{dataTypeEntityTable.TableName}_{primaryKeyField.Name}", null)
					{
						DataType = primaryKeyField.DataType
					});
			}
		}
	}
}
