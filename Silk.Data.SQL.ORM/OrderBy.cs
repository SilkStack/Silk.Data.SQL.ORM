using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.ORM
{
	public abstract class OrderBy
	{
		public abstract QueryExpression GetExpression();
	}
}
