using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silk.Data.SQL.ORM.Queries
{
	public class QueryBuilder
	{
		protected QueryExpression Source { get; set; }
		protected List<IProjectedItem> Projections { get; }
			= new List<IProjectedItem>();

		public SelectExpression BuildSelect()
		{
			return QueryExpression.Select(
				projection: Projections.Select(q => QueryExpression.Alias(QueryExpression.Column(q.FieldName, new AliasIdentifierExpression(q.SourceName)), q.AliasName)).ToArray(),
				from: Source
				);
		}
	}

	public class QueryBuilder<T> : QueryBuilder
		where T : class
	{
	}

	public class EntityQueryBuilder<T> : QueryBuilder<T>
		where T : class
	{
		public Schema.Schema Schema { get; }
		public EntitySchema<T> EntitySchema { get; }

		public EntityQueryBuilder(Schema.Schema schema)
		{
			Schema = schema;
			EntitySchema = schema.GetEntitySchema<T>();
			if (EntitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			Source = QueryExpression.Table(EntitySchema.EntityTable.TableName);
		}

		public ProjectionMapping Project<TView>()
			where TView : class
		{
			var projectionSchema = EntitySchema;
			if (typeof(TView) != typeof(T))
			{
			}

			foreach (var projectionField in projectionSchema.ProjectionFields)
			{
				Projections.Add(projectionField);
			}

			return null;
		}
	}
}
