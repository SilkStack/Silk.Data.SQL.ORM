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
	public class EntitySelectBuilder<T> : QueryBuilderBase<T>
		where T : class
	{
		private Dictionary<string, AliasExpression> _projectionExpressions
			= new Dictionary<string, AliasExpression>();
		private List<ITableJoin> _tableJoins
			= new List<ITableJoin>();
		private QueryExpression _where;
		private QueryExpression _having;
		private QueryExpression _limit;
		private QueryExpression _offset;
		private List<QueryExpression> _orderBy = new List<QueryExpression>();
		private List<QueryExpression> _groupBy = new List<QueryExpression>();

		public EntitySelectBuilder(Schema.Schema schema) : base(schema) { }

		public EntitySelectBuilder(EntitySchema<T> schema) : base(schema) { }

		public override QueryExpression BuildQuery()
		{
			return QueryExpression.Select(
				projection: _projectionExpressions.Values.ToArray(),
				from: Source,
				joins: _tableJoins.Select(q => q.GetJoinExpression()).ToArray(),
				where: _where,
				having: _having,
				limit: _limit,
				offset: _offset,
				orderBy: _orderBy.ToArray(),
				groupBy: _groupBy.ToArray()
				);
		}

		public ValueResultMapper<TProperty> Project<TProperty>(Expression<Func<T, TProperty>> projection)
		{
			if (!SqlTypeHelper.IsSqlPrimitiveType(typeof(TProperty)))
				throw new Exception("Cannot project complex types, call Project<TView>() instead.");

			var projectionField = ResolveProjectionField(projection);
			if (projectionField != null)
			{
				AddProjection(projectionField);
				return new ValueResultMapper<TProperty>(1, SchemaFieldReference<TProperty>.Create(projectionField.AliasName));
			}

			var expressionResult = ExpressionConverter.Convert(projection);

			var mapper = Project<TProperty>(expressionResult.QueryExpression);
			AddJoins(expressionResult.RequiredJoins);
			return mapper;
		}

		public ValueResultMapper<TValue> Project<TValue>(QueryExpression queryExpression)
		{
			if (!SqlTypeHelper.IsSqlPrimitiveType(typeof(TValue)))
				throw new Exception("Cannot project complex types.");

			var aliasExpression = queryExpression as AliasExpression;
			if (aliasExpression == null)
			{
				aliasExpression = QueryExpression.Alias(queryExpression, $"__AutoAlias_{_projectionExpressions.Count}");
			}
			_projectionExpressions.Add(aliasExpression.Identifier.Identifier, aliasExpression);

			return new ValueResultMapper<TValue>(1, SchemaFieldReference<TValue>.Create(
				aliasExpression.Identifier.Identifier
				));
		}

		public ObjectResultMapper<TView> Project<TView>()
			where TView : class
		{
			var projectionSchema = EntitySchema as ProjectionSchema<TView, T>;
			if (typeof(TView) != typeof(T))
			{
				throw new NotImplementedException();
				//projectionSchema = EntitySchema.GetProjection<TView>();
			}

			foreach (var schemaField in projectionSchema.SchemaFields)
				AddProjection(schemaField);

			return new ObjectResultMapper<TView>(1, projectionSchema.Mapping);
		}

		private ISchemaField<T> ResolveProjectionField<TProperty>(Expression<Func<T, TProperty>> property)
		{
			if (property.Body is MemberExpression memberExpression)
			{
				var path = new List<string>();
				PopulatePath(property.Body, path);

				return GetProjectionField(path);
			}
			return null;
		}

		private ISchemaField<T> GetProjectionField(IEnumerable<string> path)
		{
			return EntitySchema.SchemaFields.FirstOrDefault(
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

		private void AddProjection<TProjection>(ISchemaField<TProjection> schemaField)
			where TProjection : class
		{
			if (_projectionExpressions.ContainsKey(schemaField.AliasName))
				return;

			var aliasExpression = QueryExpression.Alias(
				QueryExpression.Column(schemaField.Column.ColumnName, new AliasIdentifierExpression(schemaField.Column.SourceName)),
				schemaField.AliasName);
			_projectionExpressions.Add(schemaField.AliasName, aliasExpression);
			AddJoins(schemaField.Join);
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
			if (join == null || _tableJoins.Contains(join))
				return;
			_tableJoins.Add(join);
			AddJoins(join.DependencyJoins);
		}

		public void AndWhere(QueryExpression queryExpression)
		{
			_where = QueryExpression.CombineConditions(_where, ConditionType.AndAlso, queryExpression);
		}

		public void AndWhere(Expression<Func<T, bool>> expression)
		{
			AndWhere(ExpressionConverter.Convert(expression));
		}

		public void AndWhere(ExpressionResult condition)
		{
			AndWhere(condition.QueryExpression);
			AddJoins(condition.RequiredJoins);
		}

		public void OrWhere(QueryExpression queryExpression)
		{
			_where = QueryExpression.CombineConditions(_where, ConditionType.OrElse, queryExpression);
		}

		public void OrWhere(Expression<Func<T, bool>> expression)
		{
			OrWhere(ExpressionConverter.Convert(expression));
		}

		public void OrWhere(ExpressionResult condition)
		{
			OrWhere(condition.QueryExpression);
			AddJoins(condition.RequiredJoins);
		}

		public void AndHaving(QueryExpression queryExpression)
		{
			_having = QueryExpression.CombineConditions(_having, ConditionType.AndAlso, queryExpression);
		}

		public void AndHaving(Expression<Func<T, bool>> expression)
		{
			AndHaving(ExpressionConverter.Convert(expression));
		}

		public void AndHaving(ExpressionResult condition)
		{
			AndHaving(condition.QueryExpression);
			AddJoins(condition.RequiredJoins);
		}

		public void OrHaving(QueryExpression queryExpression)
		{
			_having = QueryExpression.CombineConditions(_having, ConditionType.OrElse, queryExpression);
		}

		public void OrHaving(Expression<Func<T, bool>> expression)
		{
			OrHaving(ExpressionConverter.Convert(expression));
		}

		public void OrHaving(ExpressionResult condition)
		{
			OrHaving(condition.QueryExpression);
			AddJoins(condition.RequiredJoins);
		}

		public void OrderBy(QueryExpression queryExpression, OrderDirection orderDirection = OrderDirection.Ascending)
		{
			if (orderDirection == OrderDirection.Descending)
				queryExpression = QueryExpression.Descending(queryExpression);
			_orderBy.Add(queryExpression);
		}

		public void OrderBy<TProperty>(Expression<Func<T, TProperty>> propertyExpression, OrderDirection orderDirection = OrderDirection.Ascending)
		{
			var expressionResult = ExpressionConverter.Convert(propertyExpression);
			OrderBy(expressionResult.QueryExpression, orderDirection);
			AddJoins(expressionResult.RequiredJoins);
		}

		public void GroupBy(QueryExpression queryExpression)
		{
			_groupBy.Add(queryExpression);
		}

		public void GroupBy<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
		{
			var expressionResult = ExpressionConverter.Convert(propertyExpression);
			GroupBy(expressionResult.QueryExpression);
			AddJoins(expressionResult.RequiredJoins);
		}

		public void Offset(int offset)
		{
			Offset(QueryExpression.Value(offset));
		}

		public void Offset(QueryExpression offset)
		{
			_offset = offset;
		}

		public void Limit(int limit)
		{
			_limit = QueryExpression.Value(limit);
		}

		public void Limit(QueryExpression queryExpression)
		{
			_limit = queryExpression;
		}
	}
}
