using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class SelectQueryBuilder<TSource>
		where TSource : new()
	{
		public EntityModel<TSource> DataModel { get; }
		private int _aliasCount = 0;

		public SelectQueryBuilder(EntityModel<TSource> dataModel)
		{
			DataModel = dataModel;
		}

		public ICollection<ORMQuery> CreateQuery(
				QueryExpression where = null,
				QueryExpression having = null,
				QueryExpression[] orderBy = null,
				QueryExpression[] groupBy = null,
				int? offset = null,
				int? limit = null
			)
		{
			return CreateQuery(DataModel, where, having, orderBy, groupBy, offset, limit);
		}

		public ICollection<ORMQuery> CreateQuery<TView>(
				QueryExpression where = null,
				QueryExpression having = null,
				QueryExpression[] orderBy = null,
				QueryExpression[] groupBy = null,
				int? offset = null,
				int? limit = null
			)
			where TView : new()
		{
			var dataModel = DataModel.Domain.GetProjectionModel<TSource, TView>();

			return CreateQuery(dataModel, where, having, orderBy, groupBy, offset, limit);
		}

		public ICollection<ORMQuery> CreateQuery<TView>(
				EntityModel<TView> model,
				QueryExpression where = null,
				QueryExpression having = null,
				QueryExpression[] orderBy = null,
				QueryExpression[] groupBy = null,
				int? offset = null,
				int? limit = null
			)
			where TView : new()
		{
			var entityTable = model.Schema.EntityTable;
			var queries = new CompositeQueryExpression();
			var projectedFields = new List<QueryExpression>();
			List<JoinExpression> joins = null;

			var entityTableAlias = QueryExpression.Alias(QueryExpression.Table(entityTable.TableName), entityTable.TableName);
			AddProjectedFields(entityTableAlias, model, projectedFields, ref joins);

			queries.Queries.Add(QueryExpression.Select(
					projectedFields.ToArray(),
					from: entityTableAlias,
					where: where,
					joins: joins?.ToArray(),
					having: having,
					orderBy: orderBy,
					groupBy: groupBy,
					offset: offset != null ? QueryExpression.Value(offset.Value) : null,
					limit: limit != null ? QueryExpression.Value(limit.Value) : null
				));

			return new[] {
				new MapResultORMQuery<TView>(queries, model)
			};
		}

		private void AddProjectedFields(AliasExpression fromAliasExpression, EntityModel model,
			List<QueryExpression> projectedFields, ref List<JoinExpression> joins,
			string aliasPath = null)
		{
			var aliasPrefix = "";
			if (aliasPath != null)
				aliasPrefix = $"{aliasPath}_";
			foreach (var field in model.Fields)
			{
				if (field.Relationship == null)
				{
					projectedFields.Add(QueryExpression.Alias(
							QueryExpression.Column(field.Storage.ColumnName, fromAliasExpression.Identifier),
							$"{aliasPrefix}{field.Name}"
						));
				}
				else
				{
					var isRelationshipForeignKeyField = field.Storage != null;

					if (isRelationshipForeignKeyField)
					{
						projectedFields.Add(QueryExpression.Alias(
							QueryExpression.Column(field.Storage.ColumnName, fromAliasExpression.Identifier),
							$"{aliasPrefix}{field.Name}"
						));
						continue;
					}

					var aliasExpression = AddJoinExpression(ref joins, field, model);
					if (aliasExpression != null)
					{
						AddProjectedFields(aliasExpression,
							field.Relationship.ProjectedModel ?? field.Relationship.ForeignModel,
							projectedFields, ref joins, $"{aliasPrefix}{field.Name}");
					}
				}
			}
		}

		private AliasExpression AddJoinExpression(ref List<JoinExpression> joins, DataField field, EntityModel model)
		{
			if (field.Relationship.RelationshipType == RelationshipType.ManyToMany)
				return null;
			if (joins == null)
				joins = new List<JoinExpression>();

			//  todo: how to support joins that don't use the primary key as the foreign relationship key
			//  todo: investigate how to perform this join for tables with composite primary keys
			var foreignEntityTable = field.Relationship.ForeignModel.Schema.EntityTable;
			var foreignPrimaryKey = field.Relationship.ForeignModel.PrimaryKeyFields.First();
			var fullEntityModel = model.Domain.DataModels.FirstOrDefault(q => q.Schema.EntityTable == model.Schema.EntityTable);
			var fullEntityField = fullEntityModel.Fields.FirstOrDefault(q => q.ModelBinding.ViewFieldPath.SequenceEqual(field.ModelBinding.ViewFieldPath));

			var foreignTableAlias = QueryExpression.Alias(QueryExpression.Table(foreignEntityTable.TableName), $"t{++_aliasCount}");
			var foreignKeyStorage = fullEntityModel.Fields.FirstOrDefault(q => q.Storage != null && q.Relationship == fullEntityField.Relationship).Storage;

			joins.Add(QueryExpression.Join(
				QueryExpression.Column(foreignKeyStorage.ColumnName, QueryExpression.Table(foreignKeyStorage.Table.TableName)),
				QueryExpression.Column(foreignPrimaryKey.Storage.ColumnName, foreignTableAlias),
				JoinDirection.Left
				));

			return foreignTableAlias;
		}
	}
}
