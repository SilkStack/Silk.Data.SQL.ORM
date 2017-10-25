using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class SelectQueryBuilder<TSource>
		where TSource : new()
	{
		public DataModel<TSource> DataModel { get; }

		public SelectQueryBuilder(DataModel<TSource> dataModel)
		{
			DataModel = dataModel;
		}

		public ICollection<QueryWithDelegate> CreateQuery<TView>(
			QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null)
			where TView : new()
		{
			var dataModel = DataModel;
			if (typeof(TView) != typeof(TSource))
			{
				dataModel = DataModel.GetSubView<TView>();
			}

			//  todo: update this to work with datamodels that span multiple tables
			var table = dataModel.Fields.First().Storage.Table;
			var results = new List<TView>();
			var resultWriters = new List<IModelReadWriter>();
			var rows = new List<IContainer>();

			var queries = new List<QueryWithDelegate>();

			queries.Add(new QueryWithDelegate(QueryExpression.Select(
					dataModel.Fields.Select(q => QueryExpression.Column(q.Storage.ColumnName)).ToArray(),
					from: QueryExpression.Table(table.TableName),
					where: where,
					having: having,
					orderBy: orderBy,
					groupBy: groupBy,
					offset: offset != null ? QueryExpression.Value(offset.Value) : null,
					limit: limit != null ? QueryExpression.Value(limit.Value) : null
				),
				queryResult =>
				{
					if (!queryResult.HasRows)
						return;

					while (queryResult.Read())
					{
						var result = new TView();
						var container = new RowContainer(dataModel.Model, dataModel);
						container.ReadRow(queryResult);
						rows.Add(container);
						resultWriters.Add(new ObjectReadWriter(typeof(TView), dataModel.Model, result));
						results.Add(result);
					}
					dataModel.MapToModelAsync(resultWriters, rows)
							.ConfigureAwait(false)
							.GetAwaiter().GetResult();
				},
				async queryResult =>
				{
					if (!queryResult.HasRows)
						return;

					while (await queryResult.ReadAsync()
						.ConfigureAwait(false))
					{
						var result = new TView();
						var container = new RowContainer(dataModel.Model, dataModel);
						container.ReadRow(queryResult);
						rows.Add(container);
						resultWriters.Add(new ObjectReadWriter(typeof(TView), dataModel.Model, result));
						results.Add(result);
					}

					await dataModel.MapToModelAsync(resultWriters, rows)
							.ConfigureAwait(false);
				}, new System.Lazy<object>(() => results)));

			return queries;
		}
	}
}
