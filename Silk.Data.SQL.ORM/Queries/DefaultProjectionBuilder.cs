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

		public Schema.Schema Schema { get; }
		public EntityModel<T> EntitySchema { get; }

		public DefaultEntityProjectionBuilder(Schema.Schema schema, EntityModel<T> entitySchema, EntityExpressionConverter<T> expressionConverter = null)
		{
			Schema = schema;
			EntitySchema = entitySchema;
			_expressionConverter = expressionConverter;
		}

		public DefaultEntityProjectionBuilder(Schema.Schema schema, EntityExpressionConverter<T> expressionConverter = null) :
			this(schema, schema.GetEntityModel<T>(), expressionConverter)
		{
		}

		public Projection Build()
		{
			return new Projection(
				_projectionExpressions.Values.ToArray(),
				_requiredJoins.ToArray()
				);
		}

		public IResultReader<TView> AddView<TView>()
			where TView : class
		{
			var modelTranscriber = EntitySchema.GetModelTranscriber<TView>();
			var mapping = modelTranscriber.Mapping;

			foreach (var entityField in modelTranscriber.SchemaToTypeHelpers.Select(q => q.From))
			{
				_projectionExpressions.Add(
					entityField.ProjectionAlias,
					new AliasExpression(
						QueryExpression.Column(entityField.Column.Name, entityField.Source.AliasIdentifierExpression),
						entityField.ProjectionAlias)
					);
				var requiredJoin = entityField.Source as Join;
				if (requiredJoin != null)
					_requiredJoins.AddJoin(requiredJoin);
			}

			return new MappingReader<TView>(mapping);
		}

		public IResultReader<TProperty> AddField<TProperty>(Expression<Func<T, TProperty>> projection)
		{
			if (SqlTypeHelper.GetDataType(typeof(TProperty)) == null)
				throw new InvalidOperationException("Complex projection types aren't currently supported.");

			var expressionResult = ExpressionConverter.Convert(projection);
			var alias = $"__AutoAlias_{_projectionExpressions.Count}";
			_projectionExpressions.Add(
				alias,
				new AliasExpression(
					expressionResult.QueryExpression,
					alias)
				);
			_requiredJoins.AddJoins(expressionResult.RequiredJoins);

			return new ValueReader<TProperty>(alias);
		}

		public IResultReader<TValue> AddField<TValue>(QueryExpression queryExpression)
		{
			if (SqlTypeHelper.GetDataType(typeof(TValue)) == null)
				throw new InvalidOperationException("Complex projection types aren't currently supported.");

			var alias = $"__AutoAlias_{_projectionExpressions.Count}";
			_projectionExpressions.Add(
				alias,
				new AliasExpression(
					queryExpression,
					alias)
				);

			return new ValueReader<TValue>(alias);
		}
	}
}
