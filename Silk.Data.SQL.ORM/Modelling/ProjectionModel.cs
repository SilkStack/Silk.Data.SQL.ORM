using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class ProjectionModel : ModelBase<IEntityField>
	{
		public Table EntityTable { get; }

		public override IEntityField[] Fields => throw new System.NotImplementedException();
	}
}
