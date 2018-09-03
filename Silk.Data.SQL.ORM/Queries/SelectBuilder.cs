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
	public class SelectBuilder : IQueryBuilder
	{
		protected QueryExpression Source { get; set; }
		protected List<AliasExpression> ProjectionExpressions { get; }
			= new List<AliasExpression>();
		protected List<IProjectedItem> Projections { get; }
			= new List<IProjectedItem>();
		protected List<ITableJoin> TableJoins { get; }
			= new List<ITableJoin>();
		protected QueryExpression Where { get; private set; }
		protected QueryExpression LimitExpression { get; private set; }

		public QueryExpression BuildQuery()
		{
			return QueryExpression.Select(
				projection: Projections.Select(q => QueryExpression.Alias(QueryExpression.Column(q.FieldName, new AliasIdentifierExpression(q.SourceName)), q.AliasName))
					.Concat(ProjectionExpressions).ToArray(),
				from: Source,
				joins: TableJoins.Select(q => CreateJoin(q)).ToArray(),
				where: Where,
				limit: LimitExpression
				);
		}

		public ValueResultMapper<T> Project<T>(QueryExpression queryExpression)
			where T : struct
		{
			if (!SqlTypeHelper.IsSqlPrimitiveType(typeof(T)))
				throw new Exception("Cannot project complex types.");

			var aliasExpression = queryExpression as AliasExpression;
			if (aliasExpression == null)
			{
				aliasExpression = QueryExpression.Alias(queryExpression, $"__AutoAlias_{ProjectionExpressions.Count}");
			}
			ProjectionExpressions.Add(aliasExpression);

			return new ValueResultMapper<T>(1, aliasExpression.Identifier.Identifier);
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

		public void Limit(int limit)
		{
			LimitExpression = QueryExpression.Value(limit);
		}

		public void Limit(QueryExpression queryExpression)
		{
			LimitExpression = queryExpression;
		}
	}

	public class SelectBuilder<T> : SelectBuilder
		where T : class
	{
	}

	public class EntitySelectBuilder<T> : SelectBuilder<T>
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

		public ValueResultMapper<TProperty> Project<TProperty>(Expression<Func<T,TProperty>> projection)
			where TProperty : struct
		{
			if (!SqlTypeHelper.IsSqlPrimitiveType(typeof(TProperty)))
				throw new Exception("Cannot project complex types, call Project<TView>() instead.");

			var projectionField = ResolveProjectionField(projection);
			if (projectionField != null)
			{
				AddProjection(projectionField);
				return new ValueResultMapper<TProperty>(1, projectionField.AliasName);
			}

			var expressionResult = ExpressionConverter.Convert(projection);

			var mapper = Project<TProperty>(expressionResult.QueryExpression);
			AddJoins(expressionResult.RequiredJoins);
			return mapper;
		}

		public ObjectResultMapper<TView> Project<TView>()
			where TView : class
		{
			var projectionSchema = EntitySchema;
			if (typeof(TView) != typeof(T))
			{
			}

			foreach (var projectionField in projectionSchema.ProjectionFields)
				AddProjection(projectionField);

			return CreateResultMapper<TView>(1, projectionSchema);
		}

		private ProjectionField ResolveProjectionField<TProperty>(Expression<Func<T, TProperty>> property)
		{
			if (property.Body is MemberExpression memberExpression)
			{
				var path = new List<string>();
				PopulatePath(property.Body, path);

				return GetProjectionField(path);
			}
			return null;
		}

		private ProjectionField GetProjectionField(IEnumerable<string> path)
		{
			return EntitySchema.ProjectionFields.FirstOrDefault(
				q => q.ModelPath.SequenceEqual(path)
				);
		}

		private void PopulatePath(Expression expression, List<string> path)
		{
			if (expression is MemberExpression memberExpression)
			{
				var parentExpr = memberExpression.Expression;
				PopulatePath(parentExpr, path);

				path.Add(memberExpression.Member.Name);
			}
		}

		private void AddProjection(ProjectionField projectionField)
		{
			if (Projections.Contains(projectionField))
				return;
			Projections.Add(projectionField);
			AddJoins(projectionField.Join);
		}

		private IEnumerable<Binding> CreateMappingBindings<TView>(EntitySchema projectionSchema)
		{
			yield return new CreateInstanceIfNull<TView>(SqlTypeHelper.GetConstructor(typeof(TView)), new[] { "." });
			foreach (var field in projectionSchema.ProjectionFields)
			{
				yield return field.GetMappingBinding();
			}
		}

		private ObjectResultMapper<TView> CreateResultMapper<TView>(int resultSetCount, EntitySchema projectionSchema)
		{
			return new ObjectResultMapper<TView>(resultSetCount,
				CreateMappingBindings<TView>(projectionSchema));
		}

		public void AndWhere(Expression<Func<T, bool>> expression)
		{
			var condition = ExpressionConverter.Convert(expression);
			AndWhere(condition.QueryExpression);
			AddJoins(condition.RequiredJoins);
		}

		public void OrWhere(Expression<Func<T, bool>> expression)
		{
			var condition = ExpressionConverter.Convert(expression);
			OrWhere(condition.QueryExpression);
			AddJoins(condition.RequiredJoins);
		}

		private void AddJoins(EntityFieldJoin[] joins)
		{
			if (joins == null || joins.Length < 1)
				return;
			foreach (var join in joins)
			{
				AddJoins(join);
			}
		}

		private void AddJoins(EntityFieldJoin join)
		{
			if (join == null || TableJoins.Contains(join))
				return;
			TableJoins.Add(join);
			AddJoins(join.DependencyJoins);
		}
	}
}
