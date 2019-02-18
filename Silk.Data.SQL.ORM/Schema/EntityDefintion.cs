using Silk.Data.Modelling;
using System;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Defines options for an entity type.
	/// </summary>
	public abstract class EntityDefinition
	{
		public abstract Type EntityType { get; }
		public abstract TypeModel TypeModel { get; }

		public string TableName { get; set; }
		public EntityDefinition SetTableName(string tableName)
		{
			TableName = tableName;
			return this;
		}

		public abstract EntityModel BuildModel(IEnumerable<EntityField> entityFields);
	}

	/// <summary>
	/// Defines options for an entity type.
	/// </summary>
	public class EntityDefinition<T> : EntityDefinition
		where T : class
	{
		public override TypeModel TypeModel { get; } = TypeModel.GetModelOf<T>();

		public override Type EntityType { get; } = typeof(T);

		public EntityDefinition()
		{
			TableName = typeof(T).Name;
		}

		public new EntityDefinition<T> SetTableName(string tableName)
		{
			TableName = tableName;
			return this;
		}

		public override EntityModel BuildModel(IEnumerable<EntityField> entityFields)
		{
			return new EntityModel<T>(entityFields, TableName);
		}
	}
}
