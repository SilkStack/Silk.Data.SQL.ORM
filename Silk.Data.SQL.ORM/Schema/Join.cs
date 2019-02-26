using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;
using System.Collections.Generic;
using System.Linq;

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
		private JoinColumnPair[] _joinColumnPairs;

		public EntityJoin(IQueryReference left, IQueryReference right, string alias) :
			base(left, right, alias)
		{
		}

		public void SetJoinColumns(IEnumerable<JoinColumnPair> joinColumnPairs)
		{
			_joinColumnPairs = joinColumnPairs.ToArray();
		}

		public override JoinExpression GetJoinExpression()
		{
			var rightIdentifier = AliasIdentifierExpression;
			var leftIdentifier = Left.AliasIdentifierExpression;

			var onCondition = default(QueryExpression);
			foreach (var columnPair in _joinColumnPairs)
			{
				var newCondition = QueryExpression.Compare(
						QueryExpression.Column(columnPair.LeftColumnName, leftIdentifier),
						ComparisonOperator.AreEqual,
						QueryExpression.Column(columnPair.RightColumnName, rightIdentifier)
						);
				onCondition = QueryExpression.CombineConditions(onCondition, ConditionType.AndAlso, newCondition);
			}

			return QueryExpression.Join(
				QueryExpression.Alias(
					Right.AliasIdentifierExpression, rightIdentifier.Identifier
					),
				onCondition,
				JoinDirection.Left
				);
		}
	}

	public class JoinColumnPair
	{
		public string LeftColumnName { get; }
		public string RightColumnName { get; }

		public JoinColumnPair(string leftColumnName, string rightColumnName)
		{
			LeftColumnName = leftColumnName;
			RightColumnName = rightColumnName;
		}
	}
}
