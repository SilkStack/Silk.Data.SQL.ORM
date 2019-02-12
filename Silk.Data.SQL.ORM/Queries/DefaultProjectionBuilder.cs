using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Queries
{
	public class DefaultEntityProjectionBuilder<T> : IEntityProjectionBuilder<T>
		where T : class
	{
		private readonly JoinCollection _requiredJoins = new JoinCollection();
		private readonly Dictionary<string, AliasExpression> _projectionExpressions
			= new Dictionary<string, AliasExpression>();

		private EntityExpressionConverter<T> _expressionConverter;
		public EntityExpressionConverter<T> ExpressionConverter
		{
			get
			{
				if (_expressionConverter == null)
					_expressionConverter = new EntityExpressionConverter<T>(Schema);
				return _expressionConverter;
			}
		}

		public Schema.Schema Schema => EntitySchema.Schema;
		public EntitySchema<T> EntitySchema { get; }

		public DefaultEntityProjectionBuilder(EntitySchema<T> entitySchema, EntityExpressionConverter<T> expressionConverter = null)
		{
			EntitySchema = entitySchema;
			_expressionConverter = expressionConverter;
		}

		public DefaultEntityProjectionBuilder(Schema.Schema schema, EntityExpressionConverter<T> expressionConverter = null)
		{
			EntitySchema = schema.GetEntitySchema<T>();
			_expressionConverter = expressionConverter;
		}

		public DefaultEntityProjectionBuilder(Schema.Schema schema, EntitySchemaDefinition<T> entitySchemaDefinition,
			EntityExpressionConverter<T> expressionConverter = null)
		{
			EntitySchema = schema.GetEntitySchema(entitySchemaDefinition);
			_expressionConverter = expressionConverter;
		}

		public Projection Build()
		{
			return new Projection(
				_projectionExpressions.Values.ToArray(),
				_requiredJoins.ToArray()
				);
		}

		public ObjectResultMapper<TView> AddClass<TView>()
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

		public ValueResultMapper<TProperty> AddField<TProperty>(System.Linq.Expressions.Expression<Func<T, TProperty>> projection)
		{
			if (!SqlTypeHelper.IsSqlPrimitiveType(typeof(TProperty)))
				throw new Exception("Cannot project complex types, call AddClass<TView>() instead.");

			var projectionField = ResolveProjectionField(projection);
			if (projectionField != null)
			{
				AddProjection(projectionField);
				return new ValueResultMapper<TProperty>(1, SchemaFieldReference<TProperty>.Create(projectionField.AliasName));
			}

			var expressionResult = ExpressionConverter.Convert(projection);

			var mapper = AddField<TProperty>(expressionResult.QueryExpression);
			_requiredJoins.AddJoins(expressionResult.RequiredJoins);
			return mapper;
		}

		public ValueResultMapper<TValue> AddField<TValue>(QueryExpression queryExpression)
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

		private SchemaField<T> ResolveProjectionField<TProperty>(Expression<Func<T, TProperty>> property)
		{
			if (property.Body is MemberExpression memberExpression)
			{
				var path = new List<string>();
				PopulatePath(property.Body, path);

				return GetProjectionField(path);
			}
			return null;
		}

		private SchemaField<T> GetProjectionField(IEnumerable<string> path)
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

		private void AddProjection<TProjection>(SchemaField<TProjection> schemaField)
			where TProjection : class
		{
			if (_projectionExpressions.ContainsKey(schemaField.AliasName))
				return;

			var aliasExpression = QueryExpression.Alias(
				QueryExpression.Column(schemaField.Column.ColumnName, new AliasIdentifierExpression(schemaField.Column.SourceName)),
				schemaField.AliasName);
			_projectionExpressions.Add(schemaField.AliasName, aliasExpression);
			_requiredJoins.AddJoin(schemaField.Join);
		}
	}
}
