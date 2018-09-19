using Silk.Data.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class PartialEntitySchema
	{
		public abstract Type EntityType { get; }
		public List<IEntityField> EntityFields { get; } = new List<IEntityField>();
		public string TableName { get; }

		public PartialEntitySchema(IEntityField[] primitiveFields, string tableName)
		{
			EntityFields.AddRange(primitiveFields);
			TableName = tableName;
		}

		public abstract IEntityFieldOfEntity<TEntity> CreateRelatedEntityField<TEntity>(
			string fieldName, Type fieldType, IPropertyField modelField,
			PartialEntitySchemaCollection entityPrimitiveFields, string propertyPathPrefix,
			string[] modelPath, Dictionary<IPropertyField, string> columNames = null);
	}

	public class PartialEntitySchema<T> : PartialEntitySchema
	{
		public override Type EntityType { get; } = typeof(T);

		public PartialEntitySchema(IEntityField[] primitiveFields, string tableName) :
			base(primitiveFields, tableName)
		{
		}

		public override IEntityFieldOfEntity<TEntity> CreateRelatedEntityField<TEntity>(
			string fieldName, Type fieldType, IPropertyField modelField,
			PartialEntitySchemaCollection entityPrimitiveFields, string propertyPathPrefix,
			string[] modelPath, Dictionary<IPropertyField, string> columNames = null)
		{
			var primaryKeyFields = entityPrimitiveFields.GetEntityPrimaryKeys(fieldType);
			var foreignKeys = primaryKeyFields.Select(q =>
			{
				if (columNames != null && columNames.TryGetValue(q.ModelField, out var columnName))
					return q.BuildForeignKey(propertyPathPrefix, modelPath, columnName);
				return q.BuildForeignKey(propertyPathPrefix, modelPath);
			}).ToArray();
			return new EntityField<T, TEntity>(fieldName, modelPath, KeyType.ManyToOne, foreignKeys.ToArray(), modelField);
		}
	}
}
