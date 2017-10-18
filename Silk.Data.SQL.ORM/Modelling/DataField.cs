using System;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using System.Collections.Generic;

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
			ModelBinding modelBinding, TableSchema tableSchema)
		{
			Name = name;
			DataType = dataType;
			Metadata = metadata;
			ModelBinding = modelBinding;

			//  todo: search metadata for primary key/index/autoincrement attributes
			Storage = new DataStorage(Name, GetSqlDataType(),
				tableSchema);
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

		public static DataField FromDefinition(ViewFieldDefinition definition, TableSchema tableSchema)
		{
			return new DataField(definition.Name, definition.DataType, definition.Metadata.ToArray(),
				definition.ModelBinding, tableSchema);
		}

		public static IEnumerable<DataField> FromDefinitions(IEnumerable<TableDefinition> tableDefinitions,
			IEnumerable<ViewFieldDefinition> definitions)
		{
			var fieldsToTableDictionary = new Dictionary<ViewFieldDefinition, TableSchema>();
			var fieldsToTableDefDictionary = new Dictionary<ViewFieldDefinition, TableDefinition>();
			var tableToFieldsDictionary = new Dictionary<TableDefinition, List<DataField>>();
			var dataFields = new List<DataField>();

			foreach (var tableDefinition in tableDefinitions)
			{
				var table = new TableSchema(tableDefinition.TableName, new Lazy<DataField[]>(
					() => tableToFieldsDictionary[tableDefinition].ToArray()
					));
				tableToFieldsDictionary.Add(tableDefinition, new List<DataField>());
				foreach (var fieldDefinition in tableDefinition.Fields)
				{
					fieldsToTableDictionary.Add(fieldDefinition, table);
					fieldsToTableDefDictionary.Add(fieldDefinition, tableDefinition);
				}
			}

			foreach (var viewDef in definitions)
			{
				var dataField = FromDefinition(viewDef, fieldsToTableDictionary[viewDef]);
				var tableDef = fieldsToTableDefDictionary[viewDef];
				tableToFieldsDictionary[tableDef].Add(dataField);
				dataFields.Add(dataField);
			}

			return dataFields;
		}
	}
}
