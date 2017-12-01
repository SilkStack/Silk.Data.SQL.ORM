using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public class CleanModelNameConvention : ViewConvention<ViewBuilder>
	{
		public override ViewType SupportedViewTypes => ViewType.All;
		public override bool PerformMultiplePasses => false;
		public override bool SkipIfFieldDefined => true;

		public override void MakeModelField(ViewBuilder viewBuilder, ModelField field)
		{
			if (viewBuilder.ViewDefinition.Name.EndsWith("DomainModel"))
				viewBuilder.ViewDefinition.Name = viewBuilder.ViewDefinition.Name.Substring(0, viewBuilder.ViewDefinition.Name.Length - 11);
			if (viewBuilder.ViewDefinition.Name.EndsWith("Model"))
				viewBuilder.ViewDefinition.Name = viewBuilder.ViewDefinition.Name.Substring(0, viewBuilder.ViewDefinition.Name.Length - 5);
		}
	}
}
