using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class SelectBuilder : IQueryBuilder
	{
		protected QueryExpression Source { get; set; }
		protected List<IProjectedItem> Projections { get; }
			= new List<IProjectedItem>();
		protected List<ITableJoin> TableJoins { get; }
			= new List<ITableJoin>();

		public QueryExpression BuildQuery()
		{
			return QueryExpression.Select(
				projection: Projections.Select(q => QueryExpression.Alias(QueryExpression.Column(q.FieldName, new AliasIdentifierExpression(q.SourceName)), q.AliasName)).ToArray(),
				from: Source,
				joins: TableJoins.Select(q => CreateJoin(q)).ToArray()
				);
		}

		private static JoinExpression CreateJoin(ITableJoin tableJoin)
		{
			var onCondition = default(QueryExpression);
			var leftSource = new AliasIdentifierExpression(tableJoin.SourceName);
			var rightSource = new AliasIdentifierExpression(tableJoin.TableAlias);
			using (var leftEnumerator = ((ICollection<string>)tableJoin.LeftColumns).GetEnumerator())
			using (var rightEnumerator = ((ICollection<string>)tableJoin.RightColumns).GetEnumerator())
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
				QueryExpression.Alias(new AliasIdentifierExpression(tableJoin.TableName), tableJoin.TableAlias),
				onCondition,
				JoinDirection.Left
				);
		}
	}

	public class SelectBuilder<T> : SelectBuilder
		where T : class
	{
	}

	public class EntitySelectBuilder<T> : SelectBuilder<T>
		where T : class
	{
		public Schema.Schema Schema { get; }
		public EntitySchema<T> EntitySchema { get; }

		public EntitySelectBuilder(Schema.Schema schema)
		{
			Schema = schema;
			EntitySchema = schema.GetEntitySchema<T>();
			if (EntitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			Source = QueryExpression.Table(EntitySchema.EntityTable.TableName);
		}

		public ResultMapper Project<TView>()
			where TView : class
		{
			var projectionSchema = EntitySchema;
			if (typeof(TView) != typeof(T))
			{
			}

			foreach (var projectionField in projectionSchema.ProjectionFields)
			{
				Projections.Add(projectionField);
				//  todo: add mapping binding
			}

			TableJoins.AddRange(projectionSchema.EntityJoins);

			return null;
		}
	}
}
