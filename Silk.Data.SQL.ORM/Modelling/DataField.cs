using System;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class DataField : IViewField
	{
		public string Name { get; }
		public Type DataType { get; }
		public object[] Metadata { get; }
		public ModelBinding ModelBinding { get; }
		public DataStorage Storage { get; }

		public DataField(string name, Type dataType, object[] metadata,
			ModelBinding modelBinding)
		{
			Name = name;
			DataType = dataType;
			Metadata = metadata;
			ModelBinding = modelBinding;

			Storage = new DataStorage(Name, GetSqlDataType());
		}

		private SqlDataType GetSqlDataType()
		{
			//  todo: allow override with an attribute
			if (DataType == typeof(string))
			{
				//  todo: switch to max length text type when a size attribute is present
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
				//  todo: read size from an attribute
				var size = 0;
				return SqlDataType.Binary(size);
			}
			else if (DataType == typeof(DateTime))
			{
				return SqlDataType.DateTime();
			}

			throw new NotSupportedException($"Data type {DataType.FullName} not supported.");
		}

		public static DataField FromDefinition(ViewFieldDefinition definition)
		{
			return new DataField(definition.Name, definition.DataType, definition.Metadata.ToArray(),
				definition.ModelBinding);
		}

		public static IEnumerable<DataField> FromDefinitions(IEnumerable<ViewFieldDefinition> definitions)
		{
			return definitions.Select(q => FromDefinition(q));
		}
	}
}
