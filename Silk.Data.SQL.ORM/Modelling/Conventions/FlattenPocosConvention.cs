using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public class FlattenPocosConvention : ViewConvention<DataViewBuilder>
	{
		public override ViewType SupportedViewTypes => ViewType.All;
		public override bool PerformMultiplePasses => false;
		public override bool SkipIfFieldDefined => true;

		public override void MakeModelField(DataViewBuilder viewBuilder, ModelField field)
		{
			if (viewBuilder.IsPrimitiveType(field.DataType))
				return;

			viewBuilder.PushModel(field.Name, field.DataTypeModel);
			try
			{
				viewBuilder.ProcessModel(field.DataTypeModel);
			}
			finally
			{
				viewBuilder.PopModel();
			}
		}
	}
}
