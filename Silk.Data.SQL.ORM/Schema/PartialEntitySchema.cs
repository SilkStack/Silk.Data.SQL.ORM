using Silk.Data.Modelling;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class PartialEntitySchema
	{
		public abstract Type EntityType { get; }
		public EntityField[] PrimitiveFields { get; }
		public string TableName { get; }

		public PartialEntitySchema(EntityField[] primitiveFields, string tableName)
		{
			PrimitiveFields = primitiveFields;
			TableName = tableName;
		}

		public abstract EntityField CreateEntityField(IPropertyField modelField,
			PartialEntitySchemaCollection entityPrimitiveFields, string propertyPathPrefix);
	}

	public class PartialEntitySchema<T> : PartialEntitySchema
	{
		public override Type EntityType { get; } = typeof(T);

		public PartialEntitySchema(EntityField[] primitiveFields, string tableName) :
			base(primitiveFields, tableName)
		{
		}

		public override EntityField CreateEntityField(IPropertyField modelField,
			PartialEntitySchemaCollection entityPrimitiveFields, string propertyPathPrefix)
		{
			return new EntityField<T>(
				entityPrimitiveFields.GetEntityPrimaryKeys(modelField.FieldType).Select(q => new Column(
					$"{propertyPathPrefix}_{q.Columns[0].ColumnName}", q.Columns[0].DataType, true
					)).ToArray(),
				modelField, PrimaryKeyGenerator.NotPrimaryKey);
		}
	}
}
