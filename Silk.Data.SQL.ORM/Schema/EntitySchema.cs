﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Schema for storing and querying an entity type.
	/// </summary>
	public abstract class EntitySchema
	{
		public abstract Type EntityType { get; }
		public abstract Table EntityTable { get; }
		public abstract EntityField[] EntityFields { get; }
		public abstract ProjectionField[] ProjectionFields { get; }
		public abstract EntityJoin[] EntityJoins { get; }
	}

	/// <summary>
	/// Schema for storing and querying entities of type T.
	/// </summary>
	public class EntitySchema<T> : EntitySchema
	{
		public override EntityField[] EntityFields { get; }
		public override Table EntityTable { get; }
		public override Type EntityType { get; } = typeof(T);
		public override ProjectionField[] ProjectionFields { get; }
		public override EntityJoin[] EntityJoins { get; }

		public EntitySchema(Table entityTable, EntityField[] entityFields,
			ProjectionField[] projectionFields, EntityJoin[] manyToOneJoins)
		{
			EntityTable = entityTable;
			EntityFields = entityFields;
			ProjectionFields = projectionFields;
			EntityJoins = manyToOneJoins;
		}
	}
}
