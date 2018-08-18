using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Silk.Data.Modelling.Mapping.Binding;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Queries
{
	public class QueryBuilder : IQueryBuilder
	{
		protected QueryExpression Source;
		protected List<QueryProjection> Projection = new List<QueryProjection>();
		protected List<JoinBuilder> Joins = new List<JoinBuilder>();
		protected ConditionBuilder WhereBuilder;
		protected ConditionBuilder HavingBuilder;
		protected int? Offset;
		protected int? Limit;

		public QueryBuilder() : this(null, null) { }

		protected QueryBuilder(ConditionBuilder whereBuilder = null, ConditionBuilder havingBuilder = null)
		{
			WhereBuilder = whereBuilder ?? new ConditionBuilder();
			HavingBuilder = havingBuilder ?? new ConditionBuilder();
		}

		public void AndHaving(Condition condition)
		{
			HavingBuilder.AddAnd(condition);
		}

		public void AndWhere(Condition condition)
		{
			WhereBuilder.AddAnd(condition);
		}

		public void OrHaving(Condition condition)
		{
			HavingBuilder.AddOr(condition);
		}

		public void OrWhere(Condition condition)
		{
			WhereBuilder.AddOr(condition);
		}

		public SelectExpression CreateSelect()
		{
			return QueryExpression.Select(
				projection: Projection.Select(q => q.BuildExpression()).ToArray(),
				from: Source,
				where: WhereBuilder?.BuildExpression(),
				having: HavingBuilder?.BuildExpression()
				);
		}

		void IQueryBuilder.Source(QueryExpression source)
		{
			Source = source;
		}

		void IQueryBuilder.Offset(int? offset)
		{
			Offset = offset;
		}

		void IQueryBuilder.Limit(int? limit)
		{
			Limit = limit;
		}
	}

	public class QueryBuilder<T> : QueryBuilder
		where T : class
	{
	}

	public class EntityQueryBuilder<TEntity> : QueryBuilder<TEntity>, IEntityQueryBuilder<TEntity>
		where TEntity : class
	{
		public EntityModel<TEntity> EntityModel { get; }

		public EntityQueryBuilder(Schema.Schema schema)
		{
			EntityModel = schema.GetEntityModel<TEntity>();
			Source = QueryExpression.Table(EntityModel.EntityTable.TableName);
		}

		public IProjectionMapping<TView> Project<TView>()
			where TView : class
		{
			IProjectionModel projectionModel = EntityModel;
			if (typeof(TView) != typeof(TEntity))
				projectionModel = EntityModel.GetProjection<TView>();

			var bindings = new List<Binding>();
			bindings.Add(projectionModel.GetCreateInstanceAsNeededBinding(new[] { "." }));
			foreach (var field in projectionModel.Fields)
			{
				AddFieldToProjection(bindings, field);
			}

			return new BindingProjection<TView>(bindings);
		}

		private void AddFieldToProjection(List<Binding> bindings, IEntityField field,
			string aliasPrefix = null, string tableAlias = null, string[] writePath = null)
		{
			if (writePath == null)
				writePath = new string[0];

			if (field is IValueField valueField)
			{
				var propertyName = field.FieldName;
				var columnName = field.FieldName;

				var sourceName = tableAlias ?? EntityModel.EntityTable.TableName;
				var columnAlias = aliasPrefix == null ? field.FieldName : $"{aliasPrefix}_{field.FieldName}";
				var projection = new QueryProjection(sourceName, columnName, columnAlias);
				Projection.Add(projection);
				bindings.Add(valueField.CreateCopyBinding(columnAlias, writePath.Concat(new[] { field.FieldName }).ToArray()));
			}
			else if (field is IEmbeddedObjectField embeddedObjectField)
			{
				var sourceName = tableAlias ?? EntityModel.EntityTable.TableName;
				var embeddedAliasPrefix = aliasPrefix == null ? field.FieldName : $"{aliasPrefix}_{field.FieldName}";
				var subWritePath = writePath.Concat(new[] { field.FieldName }).ToArray();
				//  select null-check field
				var columnAlias = aliasPrefix == null ? field.FieldName : $"{aliasPrefix}_{field.FieldName}";
				var projection = new QueryProjection(sourceName, embeddedObjectField.NullCheckColumn.ColumnName, columnAlias);
				Projection.Add(projection);
				bindings.Add(embeddedObjectField.CreateNullCheckBinding(columnAlias, subWritePath));
				//  iterate over fields and add them to the projection as needed
				foreach (var subField in embeddedObjectField.EmbeddedFields)
				{
					AddFieldToProjection(bindings, subField, embeddedAliasPrefix, tableAlias, subWritePath);
				}
			}
			else if (field is ISingleRelatedObjectField relatedObjectField)
			{
				//  create a join aliased to the property name
				var joinAlias = aliasPrefix == null ? field.FieldName : $"{aliasPrefix}_{field.FieldName}";
				var subWritePath = writePath.Concat(new[] { field.FieldName }).ToArray();
				//  iterate over fields and add them to the projection as needed
				foreach (var subField in relatedObjectField.RelatedObjectProjection.Fields)
				{
					//AddFieldToProjection(bindings, subField, joinAlias, tableAlias, subWritePath);
				}
			}
		}
	}
}
