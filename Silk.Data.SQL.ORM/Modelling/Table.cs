using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Providers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class Table
	{
		public string TableName { get; private set; }
		public bool IsEntityTable { get; private set; }
		public DataField[] DataFields { get; private set; }

		internal Table()
		{
		}

		public Table(string tableName, bool isEntityTable, DataField[] dataFields)
		{
			TableName = tableName;
			IsEntityTable = isEntityTable;
			DataFields = dataFields;
		}

		internal void Initialize(string tableName, bool isEntityTable, DataField[] dataFields)
		{
			TableName = tableName;
			IsEntityTable = isEntityTable;
			DataFields = dataFields;
		}

		private CreateTableExpression CreateTableExpression()
		{
			//  todo: support indexes
			return QueryExpression.CreateTable(TableName,
				DataFields.Select(dataField =>
				{
					return new ColumnDefinitionExpression(dataField.Storage.ColumnName, dataField.Storage.DataType,
						dataField.Storage.IsNullable, dataField.Storage.IsAutoIncrement,
						dataField.Storage.IsPrimaryKey);
				}));
		}

		private DropExpression DropTableExpression()
		{
			return QueryExpression.DropTable(TableName);
		}

		private TableExistsVirtualFunctionExpression TableExistsExpression()
		{
			return QueryExpression.TableExists(TableName);
		}

		public void Create(IDataProvider dataProvider)
		{
			dataProvider.ExecuteNonQuery(CreateTableExpression());
		}

		public Task CreateAsync(IDataProvider dataProvider)
		{
			return dataProvider.ExecuteNonQueryAsync(CreateTableExpression());
		}

		public void Drop(IDataProvider dataProvider)
		{
			dataProvider.ExecuteNonQuery(DropTableExpression());
		}

		public Task DropAsync(IDataProvider dataProvider)
		{
			return dataProvider.ExecuteNonQueryAsync(DropTableExpression());
		}

		public bool Exists(IDataProvider dataProvider)
		{
			using (var queryResult = dataProvider.ExecuteReader(TableExistsExpression()))
			{
				if (!queryResult.HasRows)
					return false;
				if (!queryResult.Read())
					return false;
				return queryResult.GetInt32(0) == 1;
			}
		}

		public async Task<bool> ExistsAsync(IDataProvider dataProvider)
		{
			using (var queryResult = await dataProvider.ExecuteReaderAsync(TableExistsExpression())
				.ConfigureAwait(false))
			{
				if (!queryResult.HasRows)
					return false;
				if (!await queryResult.ReadAsync().ConfigureAwait(false))
					return false;
				return queryResult.GetInt32(0) == 1;
			}
		}
	}
}
