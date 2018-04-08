using System.Collections.Generic;
using System.Threading.Tasks;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Queries;

namespace Silk.Data.SQL.ORM.Operations
{
	public class CreateTableOperation : DataOperation
	{
		private readonly CreateTableExpression _expression;

		public override bool CanBeBatched => true;

		public CreateTableOperation(CreateTableExpression createTableExpression)
		{
			_expression = createTableExpression;
		}

		public override QueryExpression GetQuery() => _expression;

		public override void ProcessResult(QueryResult queryResult)
		{
		}

		public override Task ProcessResultAsync(QueryResult queryResult)
		{
			return Task.CompletedTask;
		}

		public static CreateTableOperation Create(Table table)
		{
			return new CreateTableOperation(QueryExpression.CreateTable(
				table.TableName,
				GetColumnDefinitions(table)
				));
		}

		private static IEnumerable<ColumnDefinitionExpression> GetColumnDefinitions(Table table)
		{
			foreach (var column in table.Columns)
			{
				yield return QueryExpression.DefineColumn(
					column.ColumnName, column.SqlDataType,
					column.IsNullable, column.IsServerGenerated && column.IsPrimaryKey,
					column.IsPrimaryKey
					);
			}
		}
	}
}
