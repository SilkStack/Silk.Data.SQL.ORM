using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Operations.Expressions;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Queries;

namespace Silk.Data.SQL.ORM.Operations
{
	public class CreateTableOperation : DataOperation
	{
		private readonly QueryExpression _expression;

		public override bool CanBeBatched => true;

		public CreateTableOperation(QueryExpression createTableExpression)
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
			var createTableExpression = QueryExpression.CreateTable(
				table.TableName,
				GetColumnDefinitions(table)
				);
			if (!table.Columns.Any(q => q.Index != null))
				return new CreateTableOperation(createTableExpression);

			var queries = new List<QueryExpression>();
			foreach (var group in table.Columns.Where(q => q.Index != null)
				.GroupBy(q => q.Index.Name))
			{
				queries.Add(QueryExpression.CreateIndex(
					table.TableName,
					uniqueConstraint: group.Any(q => q.Index.Option == IndexOption.Unique),
					columns: group.Select(q => q.ColumnName).ToArray()
					));
			}
			return new CreateTableOperation(new CompositeQueryExpression(queries.ToArray()));
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
