using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class ProjectionModel : ModelBase<IEntityField>
	{
		public Table EntityTable { get; }
		public Mapping Mapping { get; protected set; }

		public override IEntityField[] Fields { get; }

		public ProjectionModel(IEntityField[] fields, Table entityTable)
		{
			EntityTable = entityTable;
			Fields = fields;
		}
	}
}
