using Silk.Data.SQL.ORM.Modelling;

namespace Silk.Data.SQL.ORM.Queries
{
	public class SelectQueryBuilder<TSource>
		where TSource : new()
	{
		public EntityModel<TSource> DataModel { get; }

		public SelectQueryBuilder(EntityModel<TSource> dataModel)
		{
			DataModel = dataModel;
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
