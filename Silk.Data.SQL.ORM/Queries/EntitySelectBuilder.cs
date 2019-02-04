using Silk.Data.SQL.Expressions;
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

		public IEntityConditionBuilder<T> Where { get; set; }
		public IEntityConditionBuilder<T> Having { get; set; }
		public IEntityRangeBuilder<T> Range { get; set; }
		public IEntityGroupByBuilder<T> GroupBy { get; set; }
		public IEntityOrderByBuilder<T> OrderBy { get; set; }

		public EntitySelectBuilder(Schema.Schema schema) : base(schema)
		{
		}

		public EntitySelectBuilder(EntitySchema<T> schema) : base(schema)
		{
		}

		private void ConfigureBuilder()
		{
			Where = new DefaultEntityConditionBuilder<T>(EntitySchema, ExpressionConverter);
			Having = new DefaultEntityConditionBuilder<T>(EntitySchema, ExpressionConverter);
			Range = new DefaultEntityRangeBuilder<T>(EntitySchema, ExpressionConverter);
			GroupBy = new DefaultEntityGroupByBuilder<T>(EntitySchema, ExpressionConverter);
			OrderBy = new DefaultEntityOrderByBuilder<T>(EntitySchema, ExpressionConverter);
		}

		public override QueryExpression BuildQuery()
		{
			var where = Where?.Build();
			var having = Having?.Build();
			var limit = Range?.BuildLimit();
			var offset = Range?.BuildOffset();
			var groupBy = GroupBy?.Build();
			var orderBy = OrderBy?.Build();

			var groupByExpressions = groupBy.Select(q => q.QueryExpression).ToArray();
			var groupByJoins = groupBy.Where(q => q.RequiredJoins != null).SelectMany(q => q.RequiredJoins).ToArray();

			var orderByExpressions = orderBy.Select(q => q.QueryExpression).ToArray();
			var orderByJoins = orderBy.Where(q => q.RequiredJoins != null).SelectMany(q => q.RequiredJoins).ToArray();

			var joins = new List<ITableJoin>();
			joins.AddRange(_tableJoins); //  remove me
			if (where?.RequiredJoins != null && where.RequiredJoins.Length > 0)
				joins.AddRange(where.RequiredJoins.Where(join => !joins.Contains(join)));
			if (having?.RequiredJoins != null && having.RequiredJoins.Length > 0)
				joins.AddRange(having.RequiredJoins.Where(join => !joins.Contains(join)));
			if (limit?.RequiredJoins != null && limit.RequiredJoins.Length > 0)
				joins.AddRange(limit.RequiredJoins.Where(join => !joins.Contains(join)));
			if (offset?.RequiredJoins != null && offset.RequiredJoins.Length > 0)
				joins.AddRange(offset.RequiredJoins.Where(join => !joins.Contains(join)));
			if (groupByJoins != null && groupByJoins.Length > 0)
				joins.AddRange(groupByJoins.Where(join => !joins.Contains(join)));
			if (orderByJoins != null && orderByJoins.Length > 0)
				joins.AddRange(orderByJoins.Where(join => !joins.Contains(join)));

			return QueryExpression.Select(
				projection: _projectionExpressions.Values.ToArray(),
				from: Source,
				joins: joins.Select(q => q.GetJoinExpression()).ToArray(),
				where: where?.QueryExpression,
				having: having?.QueryExpression,
				limit: limit?.QueryExpression,
				offset: offset?.QueryExpression,
				orderBy: orderByExpressions,
				groupBy: groupByExpressions
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
				projectionSchema = EntitySchema.GetProjection<TView>();
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
	}
}
