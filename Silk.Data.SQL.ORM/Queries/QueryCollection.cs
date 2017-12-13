using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.Providers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public abstract class QueryCollectionBase<TThis>
		where TThis : QueryCollectionBase<TThis>
	{
		protected DataDomain DataDomain { get; }
		protected TThis Self { get; }
		protected List<ORMQuery> Queries { get; }

		protected QueryCollectionBase(DataDomain dataDomain,
			List<ORMQuery> queries = null)
		{
			DataDomain = dataDomain;
			Self = this as TThis;
			if (queries == null)
				Queries = new List<ORMQuery>();
			else
				Queries = queries;
		}

		protected EntityModel<TSource> GetEntityModel<TSource>()
			where TSource : new()
		{
			var entityModel = DataDomain.GetEntityModel<TSource>();
			if (entityModel == null)
				throw new System.InvalidOperationException($"Type '{typeof(TSource).FullName}' isn't modelled in the specified data domain.");
			return entityModel;
		}

		public TThis Insert<TSource>(params TSource[] sources)
			where TSource : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().Insert(sources)
				);
			return Self;
		}

		public TThis Insert<TSource,TView>(params TView[] sources)
			where TSource : new()
			where TView : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().Insert<TView>(sources)
				);
			return Self;
		}

		public TThis Update<TSource>(params TSource[] sources)
			where TSource : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().Update(sources)
				);
			return Self;
		}

		public TThis Update<TSource, TView>(params TView[] sources)
			where TSource : new()
			where TView : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().Update<TView>(sources)
				);
			return Self;
		}

		public TThis Delete<TSource>(params TSource[] sources)
			where TSource : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().Delete(sources)
				);
			return Self;
		}

		public TThis Delete<TSource, TView>(params TView[] sources)
			where TSource : new()
			where TView : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().Delete<TView>(sources)
				);
			return Self;
		}

		public TThis Delete<TSource>(QueryExpression where)
			where TSource : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().Delete(where)
				);
			return Self;
		}

		protected virtual List<object> ExecuteQueries(IDataProvider dataProvider)
		{
			List<object> ret = null;
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
						var mapResult = query.MapResult(queryResult);
						if (ret == null)
							ret = new List<object>();
						ret.Add(mapResult);
					}
				}
			}
			return ret;
		}

		protected virtual async Task<List<object>> ExecuteQueriesAsync(IDataProvider dataProvider)
		{
			List<object> ret = null;
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
						var mapResult = await query.MapResultAsync(queryResult)
							.ConfigureAwait(false);
						if (ret == null)
							ret = new List<object>();
						ret.Add(mapResult);
					}
				}
			}
			return ret;
		}

		public void Execute(IDataProvider dataProvider)
		{
			ExecuteQueries(dataProvider);
		}

		public Task ExecuteAsync(IDataProvider dataProvider)
		{
			return ExecuteQueriesAsync(dataProvider);
		}
	}

	public class QueryCollection : QueryCollectionBase<QueryCollection>
	{
		public QueryCollection(DataDomain dataDomain)
			: base(dataDomain)
		{
		}

		public QueryCollection<TSource> Select<TSource>(QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null)
			where TSource : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().Select(where, having, orderBy, groupBy, offset, limit)
				);
			return new QueryCollection<TSource>(DataDomain, Queries);
		}

		public QueryCollection<TView> Select<TSource, TView>(QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null)
			where TSource : new()
			where TView : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().Select<TView>(where, having, orderBy, groupBy, offset, limit)
				);
			return new QueryCollection<TView>(DataDomain, Queries);
		}
	}

	public class QueryCollection<TResult> : QueryCollectionBase<QueryCollection<TResult>>
	{
		public QueryCollection(DataDomain dataDomain, List<ORMQuery> queries)
			: base(dataDomain, queries)
		{
		}

		public QueryCollection<TResult, TSource> Select<TSource>(QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null)
			where TSource : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().Select(where, having, orderBy, groupBy, offset, limit)
				);
			return new QueryCollection<TResult, TSource>(DataDomain, Queries);
		}

		public QueryCollection<TResult, TView> Select<TSource, TView>(QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null)
			where TSource : new()
			where TView : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().Select<TView>(where, having, orderBy, groupBy, offset, limit)
				);
			return new QueryCollection<TResult, TView>(DataDomain, Queries);
		}

		public new ICollection<TResult> Execute(IDataProvider dataProvider)
		{
			return (ICollection<TResult>)ExecuteQueries(dataProvider)[0];
		}

		public new async Task<ICollection<TResult>> ExecuteAsync(IDataProvider dataProvider)
		{
			return (ICollection<TResult>)(await ExecuteQueriesAsync(dataProvider))[0];
		}
	}

	public class QueryCollection<TResult1, TResult2> : QueryCollectionBase<QueryCollection<TResult1, TResult2>>
	{
		public QueryCollection(DataDomain dataDomain, List<ORMQuery> queries)
			: base(dataDomain, queries)
		{
		}

		public new (ICollection<TResult1>,ICollection<TResult2>) Execute(IDataProvider dataProvider)
		{
			var results = ExecuteQueries(dataProvider);
			return (
				(ICollection<TResult1>)results[0],
				(ICollection<TResult2>)results[1]
				);
		}

		public new async Task<(ICollection<TResult1>, ICollection<TResult2>)> ExecuteAsync(IDataProvider dataProvider)
		{
			var results = await ExecuteQueriesAsync(dataProvider).ConfigureAwait(false);
			return (
				(ICollection<TResult1>)results[0],
				(ICollection<TResult2>)results[1]
				);
		}
	}
}
