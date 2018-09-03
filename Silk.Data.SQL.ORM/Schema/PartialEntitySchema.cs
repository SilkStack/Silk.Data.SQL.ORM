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

		public abstract IEntityField CreateRelatedEntityField<TEntity>(IPropertyField modelField,
			PartialEntitySchemaCollection entityPrimitiveFields, string propertyPathPrefix,
			string[] modelPath);
	}

	public class PartialEntitySchema<T> : PartialEntitySchema
	{
		public override Type EntityType { get; } = typeof(T);

		public PartialEntitySchema(IEntityField[] primitiveFields, string tableName) :
			base(primitiveFields, tableName)
		{
		}

		public override IEntityField CreateRelatedEntityField<TEntity>(IPropertyField modelField,
			PartialEntitySchemaCollection entityPrimitiveFields, string propertyPathPrefix,
			string[] modelPath)
		{
			var primaryKeyFields = entityPrimitiveFields.GetEntityPrimaryKeys(modelField.FieldType);
			var foreignKeys = primaryKeyFields.Select(q => q.BuildForeignKey(propertyPathPrefix, modelPath)).ToArray();
			return new EntityField<T, TEntity>(modelField, modelPath, KeyType.ManyToOne, foreignKeys.ToArray());

			//return new EntityField<T, TEntity>(
			//	entityPrimitiveFields.GetEntityPrimaryKeys(modelField.FieldType).Select(q => new Column(
			//		$"FK_{propertyPathPrefix}_{q.Columns[0].ColumnName}", q.Columns[0].DataType, true
			//		)).ToArray(),
			//	modelField, PrimaryKeyGenerator.NotPrimaryKey, modelPath, KeyType.ManyToOne,
			//	entityPrimitiveFields.GetEntityPrimaryKeys(modelField.FieldType).ToArray()
			//	);
		}
	}
}
