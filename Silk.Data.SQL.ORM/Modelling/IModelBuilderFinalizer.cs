using Silk.Data.SQL.ORM.Schema;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Modelling
{
	public interface IModelBuilderFinalizer
	{
		void FinalizeBuiltModel(Schema.Schema finalizingSchema, List<Table> tables);
	}
}
