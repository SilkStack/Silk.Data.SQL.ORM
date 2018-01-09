using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using Silk.Data.Modelling.Conventions;
using System.Linq;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public class ProjectManyToManyObjectsConvention : ViewConvention<DataViewBuilder>
	{
		public override ViewType SupportedViewTypes => ViewType.ModelDriven;
		public override bool PerformMultiplePasses => false;
		public override bool SkipIfFieldDefined => true;

		public override void MakeModelField(DataViewBuilder viewBuilder, ModelField field)
		{
			if (field.DataType.GetTypeInfo().IsValueType) return;

			var sourceField = viewBuilder.FindSourceField(field, field.Name);
			if (sourceField == null || sourceField.BindingDirection == BindingDirection.None)
				return;

			var relationshipDefinition = field.Metadata.OfType<RelationshipDefinition>().FirstOrDefault();
			if (relationshipDefinition == null || relationshipDefinition.RelationshipType != RelationshipType.ManyToMany)
				return;

			viewBuilder.DefineManyToManyViewField(sourceField,
				metadata: sourceField.Field.Metadata);
		}
	}
}
