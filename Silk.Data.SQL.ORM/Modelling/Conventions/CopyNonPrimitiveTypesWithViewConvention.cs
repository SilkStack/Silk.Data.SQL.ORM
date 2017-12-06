using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	/// <summary>
	/// Copies non-primitive types but only when modelling to a view, not convention driven modelling.
	/// </summary>
	public class CopyNonPrimitiveTypesWithViewConvention : ViewConvention<DataViewBuilder>
	{
		public override ViewType SupportedViewTypes => ViewType.ModelDriven;
		public override bool PerformMultiplePasses => false;
		public override bool SkipIfFieldDefined => true;

		public override void MakeModelField(DataViewBuilder viewBuilder, ModelField field)
		{
			if (viewBuilder.IsPrimitiveType(field.DataType))
				return;

			var sourceField = viewBuilder.FindSourceField(field, field.Name, dataType: field.DataType);
			if (sourceField == null)
				return;

			viewBuilder.DefineAssignedViewField(sourceField, metadata: field.Metadata);
		}
	}
}
