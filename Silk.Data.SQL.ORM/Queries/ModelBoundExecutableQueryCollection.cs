using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.Providers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public class ModelBoundExecutableQueryCollection<TSource> : ExecutableQueryCollection
		where TSource : new()
	{
		public EntityModel<TSource> DataModel { get; }

		public ModelBoundExecutableQueryCollection(EntityModel<TSource> dataModel, params ORMQuery[] queryExpressions)
			: base(queryExpressions)
		{
			DataModel = dataModel;
		}

		public ModelBoundExecutableQueryCollection(EntityModel<TSource> dataModel, IEnumerable<ORMQuery> queryExpressions)
			: base(queryExpressions)
		{
			DataModel = dataModel;
		}

		public ModelBoundExecutableQueryCollection<TSource> Insert(params TSource[] sources)
		{
			var insertBuilder = new InsertQueryBuilder<TSource>(DataModel);
			Queries.AddRange(insertBuilder.CreateQuery(sources));
			return this;
		}

		public ModelBoundExecutableQueryCollection<TSource> Insert<TView>(params TView[] sources)
			where TView : new()
		{
			var insertBuilder = new InsertQueryBuilder<TSource>(DataModel);
			Queries.AddRange(insertBuilder.CreateQuery<TView>(sources));
			return this;
		}

		public ModelBoundExecutableQueryCollection<TSource> Update(params TSource[] sources)
		{
			var queryuBuilder = new UpdateQueryBuilder<TSource>(DataModel);
			Queries.AddRange(queryuBuilder.CreateQuery(sources));
			return this;
		}

		public ModelBoundExecutableQueryCollection<TSource> Delete(params TSource[] sources)
		{
			var queryuBuilder = new DeleteQueryBuilder<TSource>(DataModel);
			Queries.AddRange(queryuBuilder.CreateQuery(sources));
			return this;
		}

		public ModelBoundExecutableQueryCollection<TSource> Delete(QueryExpression where = null)
		{
			var queryuBuilder = new DeleteQueryBuilder<TSource>(DataModel);
			Queries.AddRange(queryuBuilder.CreateQuery(where));
			return this;
		}

		public ModelBoundExecutableQueryCollection<TSource, TSource> Select(
			QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null)
		{
			var queryBuilder = new SelectQueryBuilder<TSource>(DataModel);
			return new ModelBoundExecutableQueryCollection<TSource, TSource>(DataModel,
				Queries.Concat(queryBuilder.CreateQuery(
					where, having, orderBy, groupBy, offset, limit
					)).ToArray());
		}

		public ModelBoundExecutableQueryCollection<TSource, TView> Select<TView>(
			QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null)
			where TView : new()
		{
			var queryBuilder = new SelectQueryBuilder<TSource>(DataModel);
			return new ModelBoundExecutableQueryCollection<TSource, TView>(DataModel,
				Queries.Concat(queryBuilder.CreateQuery<TView>(
					where, having, orderBy, groupBy, offset, limit
					)).ToArray());
		}
	}

	public class ModelBoundExecutableQueryCollection<TSource, TQueryResult> : ModelBoundExecutableQueryCollection<TSource>
		where TSource : new()
		where TQueryResult : new()
	{
		public ModelBoundExecutableQueryCollection(EntityModel<TSource> dataModel, params ORMQuery[] queryExpressions)
			: base(dataModel, queryExpressions)
		{
		}

		public ModelBoundExecutableQueryCollection(EntityModel<TSource> dataModel, IEnumerable<ORMQuery> queryExpressions)
			: base(dataModel, queryExpressions)
		{
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult> Insert(params TSource[] sources)
		{
			var insertBuilder = new InsertQueryBuilder<TSource>(DataModel);
			Queries.AddRange(insertBuilder.CreateQuery(sources));
			return this;
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult> Update(params TSource[] sources)
		{
			var queryuBuilder = new UpdateQueryBuilder<TSource>(DataModel);
			Queries.AddRange(queryuBuilder.CreateQuery(sources));
			return this;
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult> Delete(params TSource[] sources)
		{
			var queryuBuilder = new DeleteQueryBuilder<TSource>(DataModel);
			Queries.AddRange(queryuBuilder.CreateQuery(sources));
			return this;
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult> Delete(QueryExpression where = null)
		{
			var queryuBuilder = new DeleteQueryBuilder<TSource>(DataModel);
			Queries.AddRange(queryuBuilder.CreateQuery(where));
			return this;
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult, TSource> Select(
			QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null)
		{
			var queryBuilder = new SelectQueryBuilder<TSource>(DataModel);
			return new ModelBoundExecutableQueryCollection<TSource, TQueryResult, TSource>(DataModel,
				Queries.Concat(queryBuilder.CreateQuery(
					where, having, orderBy, groupBy, offset, limit
					)).ToArray());
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult, TView> Select<TView>(
			QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null)
			where TView : new()
		{
			var queryBuilder = new SelectQueryBuilder<TSource>(DataModel);
			return new ModelBoundExecutableQueryCollection<TSource, TQueryResult, TView>(DataModel,
				Queries.Concat(queryBuilder.CreateQuery<TView>(
					where, having, orderBy, groupBy, offset, limit
					)).ToArray());
		}

		public new TransactionQueryCollection<TQueryResult> AsTransaction()
		{
			return new TransactionQueryCollection<TQueryResult>(
				Queries.ToArray()
				);
		}

		public new ICollection<TQueryResult> Execute(IDataProvider dataProvider)
		{
			ICollection<TQueryResult> ret = null;
			foreach (var query in Queries)
			{
				if (query.MapToType == null)
				{
					dataProvider.ExecuteNonQuery(query.Query);
				}
				else
				{
					using (var queryResult = dataProvider.ExecuteReader(query.Query))
					{
						var result = query.MapResult(queryResult);
						if (query.IsQueryResult)
						{
							ret = result as ICollection<TQueryResult>;
						}
					}
				}
			}
			return ret;
		}

		public new async Task<ICollection<TQueryResult>> ExecuteAsync(IDataProvider dataProvider)
		{
			ICollection<TQueryResult> ret = null;
			foreach (var query in Queries)
			{
				if (query.MapToType == null)
				{
					await dataProvider.ExecuteNonQueryAsync(query.Query)
						.ConfigureAwait(false);
				}
				else
				{
					using (var queryResult = await dataProvider.ExecuteReaderAsync(query.Query)
						.ConfigureAwait(false))
					{
						var result = await query.MapResultAsync(queryResult)
							.ConfigureAwait(false);
						if (query.IsQueryResult)
						{
							ret = result as ICollection<TQueryResult>;
						}
					}
				}
			}
			return ret;
		}
	}

	public class ModelBoundExecutableQueryCollection<TSource, TQueryResult1, TQueryResult2> : ModelBoundExecutableQueryCollection<TSource>
		where TSource : new()
		where TQueryResult1 : new()
		where TQueryResult2 : new()
	{
		public ModelBoundExecutableQueryCollection(EntityModel<TSource> dataModel, params ORMQuery[] queryExpressions)
			: base(dataModel, queryExpressions)
		{
		}

		public ModelBoundExecutableQueryCollection(EntityModel<TSource> dataModel, IEnumerable<ORMQuery> queryExpressions)
			: base(dataModel, queryExpressions)
		{
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult1, TQueryResult2> Insert(params TSource[] sources)
		{
			var insertBuilder = new InsertQueryBuilder<TSource>(DataModel);
			Queries.AddRange(insertBuilder.CreateQuery(sources));
			return this;
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult1, TQueryResult2> Update(params TSource[] sources)
		{
			var queryuBuilder = new UpdateQueryBuilder<TSource>(DataModel);
			Queries.AddRange(queryuBuilder.CreateQuery(sources));
			return this;
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult1, TQueryResult2> Delete(params TSource[] sources)
		{
			var queryuBuilder = new DeleteQueryBuilder<TSource>(DataModel);
			Queries.AddRange(queryuBuilder.CreateQuery(sources));
			return this;
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult1, TQueryResult2> Delete(QueryExpression where = null)
		{
			var queryuBuilder = new DeleteQueryBuilder<TSource>(DataModel);
			Queries.AddRange(queryuBuilder.CreateQuery(where));
			return this;
		}

		public new TransactionQueryCollection<TQueryResult1, TQueryResult2> AsTransaction()
		{
			return new TransactionQueryCollection<TQueryResult1, TQueryResult2>(
				Queries.ToArray()
				);
		}

		public new(ICollection<TQueryResult1> Result1, ICollection<TQueryResult2> Result2) Execute(IDataProvider dataProvider)
		{
			ICollection<TQueryResult1> result1 = null;
			ICollection<TQueryResult2> result2 = null;
			foreach (var query in Queries)
			{
				if (query.MapToType == null)
				{
					dataProvider.ExecuteNonQuery(query.Query);
				}
				else
				{
					using (var queryResult = dataProvider.ExecuteReader(query.Query))
					{
						var result = query.MapResult(queryResult);
						if (query.IsQueryResult)
						{
							if (result1 == null)
							{
								result1 = result as ICollection<TQueryResult1>;
							}
							else if (result2 == null)
							{
								result2 = result as ICollection<TQueryResult2>;
							}
						}
					}
				}
			}
			return (result1, result2);
		}

		public new async Task<(ICollection<TQueryResult1> Result1, ICollection<TQueryResult2> Result2)> ExecuteAsync(IDataProvider dataProvider)
		{
			ICollection<TQueryResult1> result1 = null;
			ICollection<TQueryResult2> result2 = null;
			foreach (var query in Queries)
			{
				if (query.MapToType == null)
				{
					await dataProvider.ExecuteNonQueryAsync(query.Query)
						.ConfigureAwait(false);
				}
				else
				{
					using (var queryResult = await dataProvider.ExecuteReaderAsync(query.Query)
						.ConfigureAwait(false))
					{
						var result = await query.MapResultAsync(queryResult)
							.ConfigureAwait(false);
						if (query.IsQueryResult)
						{
							if (result1 == null)
							{
								result1 = result as ICollection<TQueryResult1>;
							}
							else if (result2 == null)
							{
								result2 = result as ICollection<TQueryResult2>;
							}
						}
					}
				}
			}
			return (result1, result2);
		}
	}
}
