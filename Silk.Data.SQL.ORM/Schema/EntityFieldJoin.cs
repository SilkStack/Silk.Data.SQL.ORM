using Silk.Data.SQL.Expressions;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Schema
{
	public class EntityFieldJoin : ITableJoin
	{
		public string TableName { get; }
		public string TableAlias { get; }
		public string SourceName { get; }
		public string[] LeftColumns { get; }
		public string[] RightColumns { get; }
		public IEntityField EntityField { get; }
		public EntityFieldJoin[] DependencyJoins { get; }

		public EntityFieldJoin(string tableName, string tableAlias,
			string sourceName, string[] leftColumns, string[] rightColumns,
			IEntityField entityField, EntityFieldJoin[] dependencyJoins = null)
		{
			TableName = tableName;
			TableAlias = tableAlias;
			SourceName = sourceName;
			LeftColumns = leftColumns;
			RightColumns = rightColumns;
			EntityField = entityField;
			DependencyJoins = dependencyJoins ?? new EntityFieldJoin[0];
		}

		public JoinExpression GetJoinExpression()
		{
			var onCondition = default(QueryExpression);
			var leftSource = new AliasIdentifierExpression(SourceName);
			var rightSource = new AliasIdentifierExpression(TableAlias);
			using (var leftEnumerator = ((ICollection<string>)LeftColumns).GetEnumerator())
			using (var rightEnumerator = ((ICollection<string>)RightColumns).GetEnumerator())
			{
				while (leftEnumerator.MoveNext() && rightEnumerator.MoveNext())
				{
					var newCondition = QueryExpression.Compare(
						QueryExpression.Column(leftEnumerator.Current, leftSource),
						ComparisonOperator.AreEqual,
						QueryExpression.Column(rightEnumerator.Current, rightSource)
						);
					onCondition = QueryExpression.CombineConditions(onCondition, ConditionType.AndAlso, newCondition);
				}
			}

			return QueryExpression.Join(
				QueryExpression.Alias(new AliasIdentifierExpression(TableName), TableAlias),
				onCondition,
				JoinDirection.Left
				);
		}
	}
}
