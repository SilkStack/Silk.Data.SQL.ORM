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
		public Schema Schema { get; internal set; }

		public abstract Type EntityType { get; }
		public abstract Table EntityTable { get; }
		public abstract SchemaIndex[] Indexes { get; }
		public abstract ProjectionField[] ProjectionFields { get; }
		public abstract EntityFieldJoin[] EntityJoins { get; }

		public IEntityField[] EntityFields { get; }

		public EntitySchema(IEntityField[] entityFields)
		{
			EntityFields = entityFields;
		}
	}

	/// <summary>
	/// Schema for storing and querying entities of type T.
	/// </summary>
	public class EntitySchema<T> : EntitySchema
	{
		public new IEntityFieldOfEntity<T>[] EntityFields { get; }
		public override Table EntityTable { get; }
		public override Type EntityType { get; } = typeof(T);
		public override SchemaIndex[] Indexes { get; }
		public override ProjectionField[] ProjectionFields { get; }
		public override EntityFieldJoin[] EntityJoins { get; }

		public EntitySchema(Table entityTable, IEntityFieldOfEntity<T>[] entityFields,
			ProjectionField[] projectionFields, EntityFieldJoin[] manyToOneJoins,
			SchemaIndex[] indexes) : base(entityFields)
		{
			EntityTable = entityTable;
			EntityFields = entityFields;
			ProjectionFields = projectionFields;
			EntityJoins = manyToOneJoins;
			Indexes = indexes;
		}
	}
}
