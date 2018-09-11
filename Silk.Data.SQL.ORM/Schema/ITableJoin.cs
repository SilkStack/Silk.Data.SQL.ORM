using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.ORM.Schema
{
	public interface ITableJoin
	{
		string TableAlias { get; }

		JoinExpression GetJoinExpression();
	}

	public interface ITableJoin<TLeft, TRight> : ITableJoin
		where TLeft : class
		where TRight : class
	{
	}
}
