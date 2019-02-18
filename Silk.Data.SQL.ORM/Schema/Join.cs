using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class Join : IQueryReference
	{
		private readonly string _alias;

		public IQueryReference Left { get; }
		public IQueryReference Right { get; }

		public AliasIdentifierExpression AliasIdentifierExpression { get; }

		public Join(IQueryReference left, IQueryReference right, string alias)
		{
			Left = left;
			Right = right;
			_alias = alias;
			AliasIdentifierExpression = new AliasIdentifierExpression(_alias);
		}

		public abstract JoinExpression GetJoinExpression();
	}

	public class EntityJoin : Join
	{
		public EntityJoin(IQueryReference left, IQueryReference right, string alias) :
			base(left, right, alias)
		{
		}

		public override JoinExpression GetJoinExpression()
		{
			throw new System.NotImplementedException();
		}
	}
}
