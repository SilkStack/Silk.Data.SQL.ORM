using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
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
			return new[]
			{
				new MapResultORMQuery<TView>(
					CreateRawQuery(model, where, having, orderBy, groupBy, offset, limit),
					model
				)
			};
		}

		public ICollection<ORMQuery> CreateCountQuery(
				QueryExpression where = null,
				QueryExpression having = null,
				QueryExpression[] groupBy = null
			)
		{
			return CreateCountQuery(DataModel, where, having, groupBy);
		}

		public ICollection<ORMQuery> CreateCountQuery<TView>(
				QueryExpression where = null,
				QueryExpression having = null,
				QueryExpression[] groupBy = null
			)
			where TView : new()
		{
			var dataModel = DataModel.Domain.GetProjectionModel<TSource, TView>();

			return CreateCountQuery(dataModel, where, having, groupBy);
		}

		public ICollection<ORMQuery> CreateCountQuery<TView>(
				EntityModel<TView> model,
				QueryExpression where = null,
				QueryExpression having = null,
				QueryExpression[] groupBy = null
			)
			where TView : new()
		{
			return new[]
			{
				new ScalarResultORMQuery<int>(
					CreateRawQuery(model, where, having, null, groupBy, null, null, true)
				)
			};
		}

		private QueryExpression CreateRawQuery<TView>(
				EntityModel<TView> model,
				QueryExpression where = null,
				QueryExpression having = null,
				QueryExpression[] orderBy = null,
				QueryExpression[] groupBy = null,
				int? offset = null,
				int? limit = null,
				bool isCountQuery = false
			)
			where TView : new()
		{
			var entityTable = model.Schema.EntityTable;
			var queries = new CompositeQueryExpression();
			var projectedFields = new List<QueryExpression>();
			List<JoinExpression> joins = null;
			Dictionary<string, AliasExpression> tableAliases = null;

			var entityTableAlias = QueryExpression.Alias(QueryExpression.Table(entityTable.TableName), entityTable.TableName);
			AddProjectedFields(entityTableAlias, model, projectedFields, ref joins, ref tableAliases);

			var mainSelect = QueryExpression.Select(
					projectedFields.ToArray(),
					from: entityTableAlias,
					where: where,
					joins: joins?.ToArray(),
					having: having,
					orderBy: orderBy,
					groupBy: groupBy,
					offset: offset != null ? QueryExpression.Value(offset.Value) : null,
					limit: limit != null ? QueryExpression.Value(limit.Value) : null
				);

			if (isCountQuery)
				return mainSelect.ToCountQuery();

			queries.Queries.Add(mainSelect);

			var manyToManyFields = model.Fields
				.Where(q => q.Storage == null && q.Relationship != null && q.Relationship.RelationshipType == RelationshipType.ManyToMany)
				.ToArray();
			if (manyToManyFields.Length > 0)
			{
				foreach (var field in manyToManyFields)
				{
					joins?.Clear();
					tableAliases?.Clear();
					projectedFields.Clear();

					foreach (var primaryKeyField in model.PrimaryKeyFields)
					{
						projectedFields.Add(QueryExpression.Alias(
							QueryExpression.Column(primaryKeyField.Storage.ColumnName, entityTableAlias.Identifier),
							primaryKeyField.Name
						));
					}

					var aliasExpression = AddJoinExpression(ref joins, ref tableAliases, field, model, field.Name, true);
					if (aliasExpression != null)
					{
						AddProjectedFields(aliasExpression,
							field.Relationship.ProjectedModel ?? field.Relationship.ForeignModel,
							projectedFields, ref joins, ref tableAliases, false, field.Name);
					}

					queries.Queries.Add(QueryExpression.Select(
							projectedFields.ToArray(),
							from: entityTableAlias,
							where: where,
							joins: joins?.ToArray(),
							having: having
						));
				}
			}

			return queries;
		}

		private void AddProjectedFields(AliasExpression fromAliasExpression, EntityModel model,
			List<QueryExpression> projectedFields, ref List<JoinExpression> joins,
			ref Dictionary<string, AliasExpression> tableAliases,
			bool doManyToManyRelations = false, string aliasPath = null)
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

					var aliasExpression = AddJoinExpression(ref joins, ref tableAliases, field, model, aliasPath, doManyToManyRelations);
					if (aliasExpression != null)
					{
						AddProjectedFields(aliasExpression,
							field.Relationship.ProjectedModel ?? field.Relationship.ForeignModel,
							projectedFields, ref joins, ref tableAliases, doManyToManyRelations, $"{aliasPrefix}{field.Name}");
					}
				}
			}
		}

		private AliasExpression AddJoinExpression(ref List<JoinExpression> joins,
			ref Dictionary<string, AliasExpression> tableAliases,
			DataField field, EntityModel model, string aliasPath,
			bool doManyToManyRelations)
		{
			if (joins == null)
				joins = new List<JoinExpression>();

			//  todo: how to support joins that don't use the primary key as the foreign relationship key
			//  todo: investigate how to perform this join for tables with composite primary keys

			if (field.Relationship.RelationshipType == RelationshipType.ManyToOne)
			{
				var fullFieldName = $"{aliasPath}{field.Name}";

				var foreignEntityTable = field.Relationship.ForeignModel.Schema.EntityTable;
				var foreignPrimaryKey = field.Relationship.ForeignModel.PrimaryKeyFields.First();
				var fullEntityModel = model.Domain.DataModels.FirstOrDefault(q => q.Schema.EntityTable == model.Schema.EntityTable);
				var fullEntityField = fullEntityModel.Fields.FirstOrDefault(q => q.ModelBinding.ViewFieldPath.SequenceEqual(field.ModelBinding.ViewFieldPath));

				var foreignTableAlias = QueryExpression.Alias(QueryExpression.Table(foreignEntityTable.TableName), $"t{++_aliasCount}");
				if (tableAliases == null)
					tableAliases = new Dictionary<string, AliasExpression>();
				tableAliases[fullFieldName] = foreignTableAlias;

				var foreignKeyStorage = fullEntityModel.Fields.FirstOrDefault(q => q.Storage != null && q.Relationship == fullEntityField.Relationship).Storage;
				QueryExpression foreignKeyStorageSource = QueryExpression.Table(foreignKeyStorage.Table.TableName);
				if (aliasPath != null && tableAliases.ContainsKey(aliasPath))
				{
					foreignKeyStorageSource = tableAliases[aliasPath];
				}

				joins.Add(QueryExpression.Join(
					QueryExpression.Column(foreignKeyStorage.ColumnName, foreignKeyStorageSource),
					QueryExpression.Column(foreignPrimaryKey.Storage.ColumnName, foreignTableAlias),
					JoinDirection.Left
					));

				return foreignTableAlias;
			}
			else if (doManyToManyRelations && field.Relationship.RelationshipType == RelationshipType.ManyToMany)
			{
				var joinTable = model.Schema.Tables.FirstOrDefault(q => q.IsJoinTableFor(model.Schema.EntityTable.EntityType, field.Relationship.ForeignModel.EntityType));
				if (joinTable == null)
					throw new InvalidOperationException($"Couldn't locate join table for '{field.Relationship.ForeignModel.EntityType.FullName}'.");

				var foreignEntityTable = field.Relationship.ForeignModel.Schema.EntityTable;
				var foreignPrimaryKey = field.Relationship.ForeignModel.PrimaryKeyFields.First();
				var localPrimaryKey = model.PrimaryKeyFields.First();
				var foreignJoinField = joinTable.DataFields.First(q => q.RelatedEntityType == field.Relationship.ForeignModel.EntityType);
				var localJoinField = joinTable.DataFields.First(q => q.RelatedEntityType == model.Schema.EntityTable.EntityType);

				var joinTableAlias = QueryExpression.Alias(QueryExpression.Table(joinTable.TableName), $"t{++_aliasCount}");
				var foreignTableAlias = QueryExpression.Alias(QueryExpression.Table(foreignEntityTable.TableName), $"t{++_aliasCount}");

				joins.Add(QueryExpression.Join(
					QueryExpression.Column(localPrimaryKey.Storage.ColumnName, QueryExpression.Table(localPrimaryKey.Storage.Table.TableName)),
					QueryExpression.Column(localJoinField.Storage.ColumnName, joinTableAlias),
					JoinDirection.Left
					));

				joins.Add(QueryExpression.Join(
					QueryExpression.Column(foreignJoinField.Storage.ColumnName, joinTableAlias),
					QueryExpression.Column(foreignPrimaryKey.Storage.ColumnName, foreignTableAlias),
					JoinDirection.Inner
					));

				return foreignTableAlias;
			}

			return null;
		}
	}
}
