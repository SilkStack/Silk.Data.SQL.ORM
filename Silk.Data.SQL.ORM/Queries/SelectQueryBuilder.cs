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
			foreach (var field in model.Schema.EntityTable.DataFields)
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
					projectedFields.Add(QueryExpression.Alias(
							QueryExpression.Column(field.Storage.ColumnName, fromAliasExpression.Identifier),
							$"{aliasPrefix}{field.Name}"
						));

					var aliasExpression = AddJoinExpression(ref joins, field);
					if (aliasExpression != null)
					{
						AddProjectedFields(aliasExpression, field.Relationship.ForeignModel,
							projectedFields, ref joins, $"{aliasPrefix}{field.Name}");
					}
				}
			}
		}

		private AliasExpression AddJoinExpression(ref List<JoinExpression> joins, DataField field)
		{
			if (field.Relationship.RelationshipType == RelationshipType.ManyToMany)
				return null;
			if (joins == null)
				joins = new List<JoinExpression>();

			//  todo: how to support joins that don't use the primary key as the foreign relationship key
			//  todo: investigate how to perform this join for tables with composite primary keys
			var foreignEntityTable = field.Relationship.ForeignModel.Schema.EntityTable;
			var foreignPrimaryKey = field.Relationship.ForeignModel.PrimaryKeyFields.First();

			var foreignTableAlias = QueryExpression.Alias(QueryExpression.Table(foreignEntityTable.TableName), $"t{++_aliasCount}");

			joins.Add(QueryExpression.Join(
				QueryExpression.Column(field.Storage.ColumnName, QueryExpression.Table(field.Storage.Table.TableName)),
				QueryExpression.Column(foreignPrimaryKey.Storage.ColumnName, foreignTableAlias),
				JoinDirection.Left
				));

			return foreignTableAlias;
		}

		//public ICollection<QueryWithDelegate> CreateQuery<TView>(
		//	QueryExpression where = null,
		//	QueryExpression having = null,
		//	QueryExpression[] orderBy = null,
		//	QueryExpression[] groupBy = null,
		//	int? offset = null,
		//	int? limit = null)
		//	where TView : new()
		//{
		//	var dataModel = DataModel;
		//	if (typeof(TView) != typeof(TSource))
		//	{
		//		dataModel = DataModel.GetSubView<TView>();
		//	}

			//	//  todo: update this to work with datamodels that span multiple tables
			//	var entityTable = dataModel.Schema.Tables.First(q => q.IsEntityTable);
			//	var results = new List<TView>();
			//	var resultWriters = new List<IModelReadWriter>();
			//	var rows = new List<IContainer>();

			//	var queries = new List<QueryWithDelegate>();

			//	//  produce a dictionary of aliases to models and their fields
			//	//  get the SQL to use those aliases
			//	//  map to objects based on aliases
			//	//  use a resource loader to perform mapping of JOINed objects

			//	var projectedFields = new List<QueryExpression>();
			//	var joins = new List<JoinExpression>();

			//	foreach (var entityField in dataModel.Fields
			//		.Where(q => q.Storage.Table == entityTable && q.Relationship == null))
			//	{
			//		projectedFields.Add(QueryExpression.Alias(
			//			QueryExpression.Column(entityField.Storage.ColumnName, QueryExpression.Table(entityTable.TableName)),
			//			entityField.Name
			//			));
			//	}

			//	foreach (var foreignField in dataModel.Fields
			//		.Where(q => q.Relationship != null && q.Relationship.RelationshipType == RelationshipType.ManyToOne))
			//	{
			//		var joinTable = QueryExpression.Alias(
			//			QueryExpression.Table(foreignField.Relationship.ForeignField.Storage.Table.TableName),
			//			foreignField.Name
			//			);
			//		var join = QueryExpression.Join(
			//				QueryExpression.Column(foreignField.Storage.ColumnName, QueryExpression.Table(foreignField.Storage.Table.TableName)),
			//				QueryExpression.Column(foreignField.Relationship.ForeignField.Storage.ColumnName,
			//					joinTable),
			//				JoinDirection.Left);
			//		joins.Add(join);

			//		foreach (var foreignObjField in foreignField.Relationship.ForeignModel.Fields)
			//		{
			//			projectedFields.Add(QueryExpression.Alias(
			//				QueryExpression.Column(foreignObjField.Storage.ColumnName, joinTable.Identifier),
			//				$"{foreignField.Name}_{foreignObjField.Name}"
			//			));
			//		}
			//	}

			//	if (joins.Count > 0)
			//	{
			//		//  todo: replace any ColumnExpression that doesn't have a Source with one
			//		//  that uses the entity table as it's source?
			//	}

			//	queries.Add(new QueryWithDelegate(QueryExpression.Select(
			//			projectedFields.ToArray(),
			//			from: QueryExpression.Table(entityTable.TableName),
			//			joins: joins.ToArray(),
			//			where: where,
			//			having: having,
			//			orderBy: orderBy,
			//			groupBy: groupBy,
			//			offset: offset != null ? QueryExpression.Value(offset.Value) : null,
			//			limit: limit != null ? QueryExpression.Value(limit.Value) : null
			//		),
			//		queryResult =>
			//		{
			//			if (!queryResult.HasRows)
			//				return;

			//			while (queryResult.Read())
			//			{
			//				var result = new TView();
			//				var container = new RowContainer(dataModel.Model, dataModel);
			//				container.ReadRow(queryResult);
			//				rows.Add(container);
			//				resultWriters.Add(new ObjectReadWriter(typeof(TView), TypeModeller.GetModelOf<TView>(), result));
			//				results.Add(result);
			//			}

			//			dataModel.MapToModelAsync(resultWriters, rows)
			//					.ConfigureAwait(false)
			//					.GetAwaiter().GetResult();
			//		},
			//		async queryResult =>
			//		{
			//			if (!queryResult.HasRows)
			//				return;

			//			while (await queryResult.ReadAsync()
			//				.ConfigureAwait(false))
			//			{
			//				var result = new TView();
			//				var container = new RowContainer(dataModel.Model, dataModel);
			//				container.ReadRow(queryResult);
			//				rows.Add(container);
			//				resultWriters.Add(new ObjectReadWriter(typeof(TView), TypeModeller.GetModelOf<TView>(), result));
			//				results.Add(result);
			//			}

			//			await dataModel.MapToModelAsync(resultWriters, rows)
			//					.ConfigureAwait(false);
			//		}, new System.Lazy<object>(() => results)));

			//	return queries;
			//}
	}
}
