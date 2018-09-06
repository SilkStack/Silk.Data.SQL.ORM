﻿using Silk.Data.Modelling.Mapping.Binding;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public class SelectBuilder<T> : QueryBuilderBase<T>
		where T : class
	{
		private List<AliasExpression> _projectionExpressions
			= new List<AliasExpression>();
		private List<IProjectedItem> _projections
			= new List<IProjectedItem>();
		private List<ITableJoin> _tableJoins
			= new List<ITableJoin>();
		private QueryExpression _where;
		private QueryExpression _having;
		private QueryExpression _limit;
		private QueryExpression _offset;
		private List<QueryExpression> _orderBy = new List<QueryExpression>();
		private List<QueryExpression> _groupBy = new List<QueryExpression>();

		public SelectBuilder(Schema.Schema schema) : base(schema) { }

		public SelectBuilder(EntitySchema<T> schema) : base(schema) { }

		public override QueryExpression BuildQuery()
		{
			return QueryExpression.Select(
				projection: _projections.Select(q => QueryExpression.Alias(QueryExpression.Column(q.FieldName, new AliasIdentifierExpression(q.SourceName)), q.AliasName))
					.Concat(_projectionExpressions).ToArray(),
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
			if (_projections.Contains(projectionField))
				return;
			_projections.Add(projectionField);
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

		public ValueResultMapper<TValue> Project<TValue>(QueryExpression queryExpression)
			where TValue : struct
		{
			if (!SqlTypeHelper.IsSqlPrimitiveType(typeof(TValue)))
				throw new Exception("Cannot project complex types.");

			var aliasExpression = queryExpression as AliasExpression;
			if (aliasExpression == null)
			{
				aliasExpression = QueryExpression.Alias(queryExpression, $"__AutoAlias_{_projectionExpressions.Count}");
			}
			_projectionExpressions.Add(aliasExpression);

			return new ValueResultMapper<TValue>(1, aliasExpression.Identifier.Identifier);
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
