using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	public class Schema
	{
		public EntityModelCollection EntityModels { get; }

		public Schema(EntityModelCollection entityModels)
		{
			EntityModels = entityModels;
		}

		public EntityModel<T> GetEntityModel<T>()
		{
			return GetEntityModel(typeof(T)) as EntityModel<T>;
		}

		public EntityModel GetEntityModel(Type entityType)
		{
			return EntityModels.FirstOrDefault(q => q.EntityType == entityType);
		}
	}
}
