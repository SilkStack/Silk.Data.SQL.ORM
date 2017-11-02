using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class DataRelationship
    {
		public DataField ForeignField { get; }
		public EntityModel ForeignModel { get; }

		public DataRelationship(DataField foreignField, EntityModel foreignModel)
		{
			ForeignField = foreignField;
			ForeignModel = foreignModel;
		}
    }
}
