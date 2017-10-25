using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public class CleanModelNameConvention : ViewConvention
	{
		public override void MakeModelFields(Model model, TypedModelField field, ViewDefinition viewDefinition)
		{
			if (viewDefinition.Name.EndsWith("DomainModel"))
				viewDefinition.Name = viewDefinition.Name.Substring(0, viewDefinition.Name.Length - 11);
			if (viewDefinition.Name.EndsWith("Model"))
				viewDefinition.Name = viewDefinition.Name.Substring(0, viewDefinition.Name.Length - 5);
		}
	}
}
