using System;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Modelling
{
	public abstract class EntityModel : ProjectionModel
	{
		public Type EntityType { get; }

		public EntityModel(Type entityType, IEntityField[] fields, Table entityTable) :
			base(fields, entityTable)
		{
			EntityType = entityType;
		}

		public ProjectionModel GetProjection<TProjection>()
		{
			return null;
		}
	}

	public class EntityModel<T> : EntityModel
	{
		public EntityModel(IEntityField[] fields, Table entityTable) :
			base(typeof(T), fields, entityTable)
		{
		}
	}
}
