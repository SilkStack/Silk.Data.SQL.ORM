using System;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using Silk.Data.Modelling.Conventions;
using Silk.Data.SQL.ORM.Modelling;
using System.Linq;

namespace Silk.Data.SQL.ORM
{
	public class DataViewBuilder : ViewBuilder
	{
		public DomainDefinition DomainDefinition { get; }
		public Type EntityType { get; }
		public Type ProjectionType { get; }

		public DataViewBuilder(Model sourceModel, Model targetModel, ViewConvention[] viewConventions,
			DomainDefinition domainDefinition, Type entityType, Type projectionType = null)
			: base(sourceModel, targetModel, viewConventions)
		{
			DomainDefinition = domainDefinition;
			EntityType = entityType;
			ProjectionType = projectionType;
		}

		public override void DefineField(string viewFieldName, ModelBinding binding, Type fieldDataType, params object[] metadata)
		{
			base.DefineField(viewFieldName, binding, fieldDataType, metadata);

			if (!DomainDefinition.IsReadOnly)
			{
				var fieldDefinition = ViewDefinition.FieldDefinitions
					.First(q => q.Name == viewFieldName);

				var schemaDefinition = GetSchemaDefinition();
				var entityTable = schemaDefinition.GetEntityTableDefinition(true);
				entityTable.Fields.Add(fieldDefinition);
			}
		}

		private SchemaDefinition GetSchemaDefinition()
		{
			var schemaDefinition = GetSchemaDefinitionFor(EntityType);
			if (schemaDefinition == null)
			{
				schemaDefinition = new SchemaDefinition(
					ViewDefinition, EntityType, ProjectionType
					);

				DomainDefinition.SchemaDefinitions.Add(schemaDefinition);
			}
			return schemaDefinition;
		}

		private SchemaDefinition GetSchemaDefinitionFor(Type entityType)
		{
			return DomainDefinition
				.SchemaDefinitions.FirstOrDefault(q => q.EntityType == entityType);
		}
	}
}
