﻿using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public class ManyToOneConvention : ViewConvention<DataViewBuilder>
	{
		public override ViewType SupportedViewTypes => ViewType.All;
		public override bool PerformMultiplePasses => true;
		public override bool SkipIfFieldDefined => false;

		public override void MakeModelField(DataViewBuilder viewBuilder, ModelField field)
		{
			if (viewBuilder.Mode == ViewType.ConventionDerived)
				MakeConventionDrivenFields(viewBuilder, field);
			else if (viewBuilder.Mode == ViewType.ModelDriven)
				MakeModelDrivenFields(viewBuilder, field);
		}

		public void MakeModelDrivenFields(DataViewBuilder viewBuilder, ModelField field)
		{
			var possiblePaths = ConventionHelpers.GetPaths(field.Name);
			if (possiblePaths == null)
				return;

			foreach (var candidatePath in possiblePaths)
			{
				var sourceField = viewBuilder.FindSourceField(field, candidatePath, dataType: field.DataType);
				if (sourceField == null)
					continue;

				var rootSourceField = viewBuilder.FindSourceField(field, candidatePath[0]);
				if (viewBuilder.IsPrimitiveType(rootSourceField.Field.DataType) || rootSourceField.Field.IsEnumerable)
					continue;

				var dataTypeSchemaDefinition = viewBuilder.GetSchemaDefinitionFor(rootSourceField.Field.DataType);
				if (dataTypeSchemaDefinition == null)
					continue;

				var dataTypeEntityTable = dataTypeSchemaDefinition.TableDefinitions.FirstOrDefault(q => q.IsEntityTable);
				if (dataTypeEntityTable == null)
					continue;

				var primaryKeyField = dataTypeEntityTable.Fields.FirstOrDefault(q => q.Name == candidatePath.Last() &&  q.Metadata.OfType<PrimaryKeyAttribute>().Any());
				if (primaryKeyField == null)
					continue;

				var fieldName = string.Join("", candidatePath);

				var existingField = viewBuilder.GetDefinedField(fieldName);
				if (existingField != null)
				{
					viewBuilder.UndefineField(existingField);
				}

				viewBuilder.DefineManyToOneViewField(sourceField, candidatePath, fieldName,
					new RelationshipDefinition
					{
						EntityType = rootSourceField.Field.DataType,
						RelationshipField = primaryKeyField.Name,
						RelationshipType = RelationshipType.ManyToOne
					},
					new IsNullableAttribute(true));
			}
		}

		public void MakeConventionDrivenFields(DataViewBuilder viewBuilder, ModelField field)
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

			foreach (var primaryKeyField in dataTypePrimaryKeyFields)
			{
				var fieldName = $"{field.Name}{primaryKeyField.Name}";
				if (viewBuilder.IsFieldDefined(fieldName))
					continue;

				var modelBindPath = new[] { field.Name, primaryKeyField.Name };
				var sourceField = viewBuilder.FindSourceField(field, modelBindPath);
				if (sourceField == null)
					continue;

				viewBuilder.DefineManyToOneViewField(sourceField, modelBindPath, fieldName,
					new RelationshipDefinition
					{
						EntityType = field.DataType,
						RelationshipField = primaryKeyField.Name,
						RelationshipType = RelationshipType.ManyToOne
					},
					new IsNullableAttribute(true));
			}
		}
	}
}
