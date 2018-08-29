using Silk.Data.Modelling.Mapping.Binding;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public class SelectBuilder : IQueryBuilder, IConditionalQuery
	{
		protected QueryExpression Source { get; set; }
		protected List<IProjectedItem> Projections { get; }
			= new List<IProjectedItem>();
		protected List<ITableJoin> TableJoins { get; }
			= new List<ITableJoin>();
		protected QueryExpression Where { get; private set; }

		public QueryExpression BuildQuery()
		{
			return QueryExpression.Select(
				projection: Projections.Select(q => QueryExpression.Alias(QueryExpression.Column(q.FieldName, new AliasIdentifierExpression(q.SourceName)), q.AliasName)).ToArray(),
				from: Source,
				joins: TableJoins.Select(q => CreateJoin(q)).ToArray(),
				where: Where
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

		public void AndWhere(QueryExpression queryExpression)
		{
			Where = QueryExpression.CombineConditions(Where, ConditionType.AndAlso, queryExpression);
		}

		public void OrWhere(QueryExpression queryExpression)
		{
			Where = QueryExpression.CombineConditions(Where, ConditionType.OrElse, queryExpression);
		}
	}

	public class SelectBuilder<T> : SelectBuilder
		where T : class
	{
	}

	public class EntitySelectBuilder<T> : SelectBuilder<T>, IEntityConditionalQuery<T>
		where T : class
	{
		private ExpressionConverter<T> _expressionConverter;
		private ExpressionConverter<T> ExpressionConverter
		{
			get
			{
				if (_expressionConverter == null)
					_expressionConverter = new ExpressionConverter<T>(Schema);
				return _expressionConverter;
			}
		}

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

		public ResultMapper<TView> Project<TView>()
			where TView : class
		{
			var projectionSchema = EntitySchema;
			if (typeof(TView) != typeof(T))
			{
			}

			Projections.AddRange(projectionSchema.ProjectionFields);
			TableJoins.AddRange(projectionSchema.EntityJoins);

			return CreateResultMapper<TView>(1, projectionSchema);
		}

		private IEnumerable<Binding> CreateMappingBindings<TView>(EntitySchema projectionSchema)
		{
			yield return new CreateInstanceIfNull<TView>(SqlTypeHelper.GetConstructor(typeof(TView)), new[] { "." });
			foreach (var field in projectionSchema.ProjectionFields)
			{
				yield return field.GetMappingBinding();
			}
		}

		private ResultMapper<TView> CreateResultMapper<TView>(int resultSetCount, EntitySchema projectionSchema)
		{
			return new ResultMapper<TView>(resultSetCount,
				CreateMappingBindings<TView>(projectionSchema));
		}

		public void AndWhere(Expression<Func<T, bool>> expression)
		{
			var condition = ExpressionConverter.Convert(expression);
			AndWhere(condition.QueryExpression);
		}

		public void OrWhere(Expression<Func<T, bool>> expression)
		{
			var condition = ExpressionConverter.Convert(expression);
			OrWhere(condition.QueryExpression);
		}
	}
}
