using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	public class Schema
	{
		public EntityModelCollection EntityModels { get; }
		public Table[] Tables { get; private set; }

		public Schema(EntityModelCollection entityModels)
		{
			EntityModels = entityModels;
		}

		internal void SetTables(IEnumerable<Table> tables)
		{
			Tables = tables.ToArray();
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
