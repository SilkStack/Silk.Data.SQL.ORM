using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Queries;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Schema
{
	public interface IFieldBuilder
	{
		IEnumerable<EntityField> BuildEntityFields<TEntity>(
			EntityDefinition rootEntityDefinition,
			EntityDefinition entityDefinition,
			TypeModel typeModel,
			IEnumerable<IField> relativeParentFields = null,
			IEnumerable<IField> fullParentFields = null,
			IQueryReference source = null
			)
			where TEntity : class;
	}
}
