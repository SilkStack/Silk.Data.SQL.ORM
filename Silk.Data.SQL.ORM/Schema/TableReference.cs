using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;

namespace Silk.Data.SQL.ORM.Schema
{
	public class TableReference : IQueryReference
	{
		public AliasIdentifierExpression AliasIdentifierExpression { get; }

		public TableReference(string tableName)
		{
			AliasIdentifierExpression = new AliasIdentifierExpression(tableName);
		}
	}
}
