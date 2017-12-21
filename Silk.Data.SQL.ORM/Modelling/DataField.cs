using System;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class DataField : IViewField
	{
		public string Name { get; protected set; }
		public Type DataType { get; protected set; }
		public object[] Metadata { get; protected set; }
		public ModelBinding ModelBinding { get; protected set; }

		public DataStorage Storage { get; protected set; }
		public DataRelationship Relationship { get; private set; }

		protected DataField() { }

		public DataField(string storageName, Type dataType, object[] metadata,
			ModelBinding modelBinding, Table tableSchema, DataRelationship relationship,
			string fieldName = null)
		{
			if (!string.IsNullOrEmpty(fieldName))
				Name = fieldName;
			else
				Name = storageName;
			DataType = dataType;
			Metadata = metadata;
			ModelBinding = modelBinding;
			Relationship = relationship;

			var isNullable = false;
			var nullableAttr = metadata.OfType<IsNullableAttribute>().FirstOrDefault();
			if (nullableAttr != null)
			{
				isNullable = nullableAttr.IsNullable;
			}
			else
			{
				isNullable = !dataType.IsValueType;
				if (!isNullable)
				{
					isNullable = dataType.IsGenericType &&
						dataType.GetGenericTypeDefinition() == typeof(Nullable<>);
				}
			}

			//  todo: search metadata for index definitions?
			if (relationship?.RelationshipType != RelationshipType.ManyToMany)
			{
				var sqlType = GetSqlDataType();
				if (sqlType != null)
				{
					Storage = new DataStorage(storageName, sqlType,
						tableSchema,
						metadata.OfType<PrimaryKeyAttribute>().Any(),
						metadata.OfType<AutoIncrementAttribute>().Any(),
						metadata.OfType<AutoGenerateIdAttribute>().Any(),
						isNullable);
				}
			}
		}

		internal void SetRelationship(DataRelationship dataRelationship)
		{
			Relationship = dataRelationship;
		}

		protected SqlDataType GetSqlDataType()
		{
			//  todo: allow override with an attribute
			if (DataType == typeof(string))
			{
				var lengthAttribute = Metadata.OfType<DataLengthAttribute>().FirstOrDefault();
				if (lengthAttribute != null)
					return SqlDataType.Text(lengthAttribute.DataLength);
				return SqlDataType.Text();
			}
			else if (DataType == typeof(bool))
			{
				return SqlDataType.Bit();
			}
			else if (DataType == typeof(byte))
			{
				return SqlDataType.TinyInt();
			}
			else if (DataType == typeof(short))
			{
				return SqlDataType.SmallInt();
			}
			else if (DataType == typeof(int))
			{
				return SqlDataType.Int();
			}
			else if (DataType == typeof(long))
			{
				return SqlDataType.BigInt();
			}
			else if (DataType == typeof(float))
			{
				return SqlDataType.Float(SqlDataType.FLOAT_MAX_PRECISION);
			}
			else if (DataType == typeof(double))
			{
				return SqlDataType.Float(SqlDataType.DOUBLE_MAX_PRECISION);
			}
			else if (DataType == typeof(decimal))
			{
				//  todo: attribute to support precision and scale
				return SqlDataType.Decimal();
			}
			else if (DataType == typeof(Guid))
			{
				return SqlDataType.Guid();
			}
			else if (DataType == typeof(byte[]))
			{
				var lengthAttribute = Metadata.OfType<DataLengthAttribute>().FirstOrDefault();
				if (lengthAttribute == null)
					throw new InvalidOperationException("A DataLength attribute is required on binary storage types.");
				return SqlDataType.Binary(lengthAttribute.DataLength);
			}
			else if (DataType == typeof(DateTime))
			{
				return SqlDataType.DateTime();
			}

			return null;
		}
	}

	/// <summary>
	/// A mutable version of DataField, only to be used when building a domain.
	/// </summary>
	public class MutableDataField : DataField
	{
		public Table Table { get; set; }

		public MutableDataField(string fieldName, Type fieldType,
			ModelBinding binding, object[] metadata)
		{
			Name = fieldName;
			DataType = fieldType;
			ModelBinding = binding;
			Metadata = metadata;
		}

		public void SetStorage()
		{
			if (Relationship?.RelationshipType != RelationshipType.ManyToMany)
			{
				var isNullable = false;
				var nullableAttr = Metadata.OfType<IsNullableAttribute>().FirstOrDefault();
				if (nullableAttr != null)
				{
					isNullable = nullableAttr.IsNullable;
				}
				else
				{
					isNullable = !DataType.IsValueType;
					if (!isNullable)
					{
						isNullable = DataType.IsGenericType &&
							DataType.GetGenericTypeDefinition() == typeof(Nullable<>);
					}
				}

				var sqlType = GetSqlDataType();
				if (sqlType != null)
				{
					Storage = new DataStorage(Name, sqlType,
						Table,
						Metadata.OfType<PrimaryKeyAttribute>().Any(),
						Metadata.OfType<AutoIncrementAttribute>().Any(),
						Metadata.OfType<AutoGenerateIdAttribute>().Any(),
						isNullable);
				}
			}
		}
	}
}
