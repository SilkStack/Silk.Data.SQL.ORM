using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Providers;
using System;
using System.Linq;
using System.Threading.Tasks;

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

		private CreateTableExpression CreateTableExpression()
		{
			//  todo: support indexes
			return QueryExpression.CreateTable(TableName,
				DataFields.Select(dataField =>
				{
					var isNullable = !dataField.DataType.IsValueType;
					if (!isNullable)
					{
						isNullable = dataField.DataType.IsGenericType &&
							dataField.DataType.GetGenericTypeDefinition() == typeof(Nullable<>);
					}
					return new ColumnDefinitionExpression(dataField.Name, dataField.Storage.DataType,
						isNullable, dataField.Storage.IsAutoIncrement, dataField.Storage.IsPrimaryKey);
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
