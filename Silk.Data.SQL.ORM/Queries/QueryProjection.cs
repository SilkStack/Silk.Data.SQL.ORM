using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public class QueryProjection : IExpressionBuilder
	{
		public string SourceAlias { get; }
		public string ColumnName { get; }
		public string ColumnAlias { get; }

		public QueryProjection(string sourceAlias, string columnName, string columnAlias)
		{
			SourceAlias = sourceAlias;
			ColumnName = columnName;
			ColumnAlias = columnAlias;
		}

		public QueryExpression BuildExpression()
		{
			return QueryExpression.Alias(
				QueryExpression.Column(ColumnName, new AliasIdentifierExpression(SourceAlias)),
				ColumnAlias);
		}
	}
}
