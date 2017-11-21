using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;
using Silk.Data.SQL.ORM.Modelling;

namespace Silk.Data.SQL.ORM
{
	public class DataViewBuilder : ViewBuilder
	{
		public DomainDefinition DomainDefinition { get; }

		public DataViewBuilder(Model sourceModel, Model targetModel, ViewConvention[] viewConventions,
			DomainDefinition domainDefinition)
			: base(sourceModel, targetModel, viewConventions)
		{
			DomainDefinition = domainDefinition;
		}
	}
}
