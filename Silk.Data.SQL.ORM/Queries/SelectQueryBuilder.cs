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

		public ICollection<QueryWithDelegate> CreateQuery(QueryExpression where = null,
			int? offset = null,
			int? limit = null)
		{
			//  todo: update this to work with datamodels that span multiple tables
			var table = DataModel.Fields.First().Storage.Table;
			var results = new List<TSource>();
			var resultWriters = new List<IModelReadWriter>();
			var rows = new List<IContainer>();

			var queries = new List<QueryWithDelegate>();

			queries.Add(new QueryWithDelegate(QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table(table.TableName),
					where: where,
					offset: offset != null ? QueryExpression.Value(offset.Value) : null,
					limit: limit != null ? QueryExpression.Value(limit.Value) : null
				),
				queryResult =>
				{
					if (!queryResult.HasRows)
						return;

					while (queryResult.Read())
					{
						var result = new TSource();
						var container = new RowContainer(DataModel.Model, DataModel);
						container.ReadRow(queryResult);
						rows.Add(container);
						resultWriters.Add(new ObjectReadWriter(typeof(TSource), DataModel.Model, result));
						results.Add(result);
					}
					DataModel.MapToModelAsync(resultWriters, rows)
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
						var result = new TSource();
						var container = new RowContainer(DataModel.Model, DataModel);
						container.ReadRow(queryResult);
						rows.Add(container);
						resultWriters.Add(new ObjectReadWriter(typeof(TSource), DataModel.Model, result));
						results.Add(result);
					}

					await DataModel.MapToModelAsync(resultWriters, rows)
							.ConfigureAwait(false);
				}, new System.Lazy<object>(() => results)));

			return queries;
		}
	}
}
