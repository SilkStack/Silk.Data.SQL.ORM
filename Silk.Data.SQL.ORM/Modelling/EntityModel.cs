using System;

namespace Silk.Data.SQL.ORM.Modelling
{
	public abstract class EntityModel : ProjectionModel
	{
		public Type EntityType { get; }

		public EntityModel(Type entityType)
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
		public EntityModel() : base(typeof(T)) { }
	}
}
