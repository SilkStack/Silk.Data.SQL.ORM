using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.ORM
{
	public abstract class Condition
	{
		public abstract QueryExpression GetExpression();
	}
}
