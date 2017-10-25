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
		public DataModel<TSource> DataModel { get; }

		public ModelBoundExecutableQueryCollection(DataModel<TSource> dataModel, params QueryWithDelegate[] queryExpressions)
			: base(queryExpressions)
		{
			DataModel = dataModel;
		}

		public ModelBoundExecutableQueryCollection(DataModel<TSource> dataModel, IEnumerable<QueryWithDelegate> queryExpressions)
			: base(queryExpressions)
		{
			DataModel = dataModel;
		}

		public ModelBoundExecutableQueryCollection<TSource> Insert(params TSource[] sources)
		{
			var queries = Queries.Concat(new InsertQueryBuilder<TSource>(DataModel).CreateQuery(sources));
			return new ModelBoundExecutableQueryCollection<TSource>(DataModel, queries);
		}

		public ModelBoundExecutableQueryCollection<TSource> Update(params TSource[] sources)
		{
			var queries = Queries.Concat(new UpdateQueryBuilder<TSource>(DataModel).CreateQuery(sources));
			return new ModelBoundExecutableQueryCollection<TSource>(DataModel, queries);
		}

		public ModelBoundExecutableQueryCollection<TSource> Delete(params TSource[] sources)
		{
			var queries = Queries.Concat(new DeleteQueryBuilder<TSource>(DataModel).CreateQuery(sources));
			return new ModelBoundExecutableQueryCollection<TSource>(DataModel, queries);
		}

		public ModelBoundExecutableQueryCollection<TSource> Delete(QueryExpression where = null)
		{
			var queries = Queries.Concat(new DeleteQueryBuilder<TSource>(DataModel).CreateQuery(where: where));
			return new ModelBoundExecutableQueryCollection<TSource>(DataModel, queries);
		}

		public ModelBoundExecutableQueryCollection<TSource, TSource> Select(
			QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null)
		{
			var queries = Queries.Concat(new SelectQueryBuilder<TSource>(DataModel).CreateQuery<TSource>(
				where, having, orderBy, groupBy, offset, limit
				));
			return new ModelBoundExecutableQueryCollection<TSource, TSource>(DataModel, queries);
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
			var queries = Queries.Concat(new SelectQueryBuilder<TSource>(DataModel).CreateQuery<TView>(
				where, having, orderBy, groupBy, offset, limit
				));
			return new ModelBoundExecutableQueryCollection<TSource, TView>(DataModel, queries);
		}
	}

	public class ModelBoundExecutableQueryCollection<TSource, TQueryResult> : ModelBoundExecutableQueryCollection<TSource>
		where TSource : new()
		where TQueryResult : new()
	{
		public ModelBoundExecutableQueryCollection(DataModel<TSource> dataModel, params QueryWithDelegate[] queryExpressions)
			: base(dataModel, queryExpressions)
		{
		}

		public ModelBoundExecutableQueryCollection(DataModel<TSource> dataModel, IEnumerable<QueryWithDelegate> queryExpressions)
			: base(dataModel, queryExpressions)
		{
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult> Insert(params TSource[] sources)
		{
			var queries = Queries.Concat(new InsertQueryBuilder<TSource>(DataModel).CreateQuery(sources));
			return new ModelBoundExecutableQueryCollection<TSource, TQueryResult>(DataModel, queries);
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult> Update(params TSource[] sources)
		{
			var queries = Queries.Concat(new UpdateQueryBuilder<TSource>(DataModel).CreateQuery(sources));
			return new ModelBoundExecutableQueryCollection<TSource, TQueryResult>(DataModel, queries);
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult> Delete(params TSource[] sources)
		{
			var queries = Queries.Concat(new DeleteQueryBuilder<TSource>(DataModel).CreateQuery(sources));
			return new ModelBoundExecutableQueryCollection<TSource, TQueryResult>(DataModel, queries);
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult> Delete(QueryExpression where = null)
		{
			var queries = Queries.Concat(new DeleteQueryBuilder<TSource>(DataModel).CreateQuery(where: where));
			return new ModelBoundExecutableQueryCollection<TSource, TQueryResult>(DataModel, queries);
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult, TSource> Select(
			QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null)
		{
			var queries = Queries.Concat(new SelectQueryBuilder<TSource>(DataModel).CreateQuery<TSource>(
				where, having, orderBy, groupBy, offset, limit
				));
			return new ModelBoundExecutableQueryCollection<TSource, TQueryResult, TSource>(DataModel, queries);
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
			var queries = Queries.Concat(new SelectQueryBuilder<TSource>(DataModel).CreateQuery<TView>(
				where, having, orderBy, groupBy, offset, limit
				));
			return new ModelBoundExecutableQueryCollection<TSource, TQueryResult, TView>(DataModel, queries);
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
				if (query.Delegate == null)
				{
					dataProvider.ExecuteNonQuery(query.Query);
				}
				else
				{
					using (var queryResult = dataProvider.ExecuteReader(query.Query))
					{
						query.Delegate(queryResult);
						if (query.Query is SelectExpression && query.AssignsResults)
							ret = query.Results as ICollection<TQueryResult>;
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
				if (query.Delegate == null)
				{
					await dataProvider.ExecuteNonQueryAsync(query.Query)
						.ConfigureAwait(false);
				}
				else
				{
					using (var queryResult = await dataProvider.ExecuteReaderAsync(query.Query)
						.ConfigureAwait(false))
					{
						await query.AsyncDelegate(queryResult)
							.ConfigureAwait(false);
						if (query.Query is SelectExpression && query.AssignsResults)
							ret = query.Results as ICollection<TQueryResult>;
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
		public ModelBoundExecutableQueryCollection(DataModel<TSource> dataModel, params QueryWithDelegate[] queryExpressions)
			: base(dataModel, queryExpressions)
		{
		}

		public ModelBoundExecutableQueryCollection(DataModel<TSource> dataModel, IEnumerable<QueryWithDelegate> queryExpressions)
			: base(dataModel, queryExpressions)
		{
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult1, TQueryResult2> Insert(params TSource[] sources)
		{
			var queries = Queries.Concat(new InsertQueryBuilder<TSource>(DataModel).CreateQuery(sources));
			return new ModelBoundExecutableQueryCollection<TSource, TQueryResult1, TQueryResult2>(DataModel, queries);
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult1, TQueryResult2> Update(params TSource[] sources)
		{
			var queries = Queries.Concat(new UpdateQueryBuilder<TSource>(DataModel).CreateQuery(sources));
			return new ModelBoundExecutableQueryCollection<TSource, TQueryResult1, TQueryResult2>(DataModel, queries);
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult1, TQueryResult2> Delete(params TSource[] sources)
		{
			var queries = Queries.Concat(new DeleteQueryBuilder<TSource>(DataModel).CreateQuery(sources));
			return new ModelBoundExecutableQueryCollection<TSource, TQueryResult1, TQueryResult2>(DataModel, queries);
		}

		public new ModelBoundExecutableQueryCollection<TSource, TQueryResult1, TQueryResult2> Delete(QueryExpression where = null)
		{
			var queries = Queries.Concat(new DeleteQueryBuilder<TSource>(DataModel).CreateQuery(where: where));
			return new ModelBoundExecutableQueryCollection<TSource, TQueryResult1, TQueryResult2>(DataModel, queries);
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
				if (query.Delegate == null)
				{
					dataProvider.ExecuteNonQuery(query.Query);
				}
				else
				{
					using (var queryResult = dataProvider.ExecuteReader(query.Query))
					{
						query.Delegate(queryResult);
						if (query.Query is SelectExpression && query.AssignsResults)
						{
							if (result1 == null)
								result1 = query.Results as ICollection<TQueryResult1>;
							else if (result2 == null)
								result2 = query.Results as ICollection<TQueryResult2>;
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
				if (query.Delegate == null)
				{
					await dataProvider.ExecuteNonQueryAsync(query.Query)
						.ConfigureAwait(false);
				}
				else
				{
					using (var queryResult = await dataProvider.ExecuteReaderAsync(query.Query)
						.ConfigureAwait(false))
					{
						await query.AsyncDelegate(queryResult)
							.ConfigureAwait(false);
						if (query.Query is SelectExpression && query.AssignsResults)
						{
							if (result1 == null)
								result1 = query.Results as ICollection<TQueryResult1>;
							else if (result2 == null)
								result2 = query.Results as ICollection<TQueryResult2>;
						}
					}
				}
			}
			return (result1, result2);
		}
	}
}
