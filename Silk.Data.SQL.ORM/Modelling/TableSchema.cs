using System;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class TableSchema
	{
		private Lazy<DataField[]> _lazyDataFields;

		public string TableName { get; }
		public DataField[] DataFields => _lazyDataFields.Value;

		public TableSchema(string tableName, Lazy<DataField[]> lazyDataFields)
		{
			TableName = tableName;
			_lazyDataFields = lazyDataFields;
		}
	}
}
