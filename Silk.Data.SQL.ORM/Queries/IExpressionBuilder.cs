using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public interface IExpressionBuilder
	{
		QueryExpression BuildExpression();
	}
}
