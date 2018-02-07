using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public abstract class QueryCollectionBase<TThis>
		where TThis : QueryCollectionBase<TThis>
	{
		[Obsolete]
		protected DataDomain DataDomain { get; }
		protected TThis Self { get; }
		protected List<ORMQuery> Queries { get; }

		public IORMQueryExecutor QueryExecutor { get; set; }

		protected QueryCollectionBase(List<ORMQuery> queries = null,
			IORMQueryExecutor queryExecutor = null)
		{
			Self = this as TThis;
			Queries = queries ?? new List<ORMQuery>();
			QueryExecutor = queryExecutor ?? new BasicQueryExecutor();
		}

		[Obsolete]
		protected QueryCollectionBase(DataDomain dataDomain,
			List<ORMQuery> queries = null, IORMQueryExecutor queryExecutor = null)
		{
			DataDomain = dataDomain;
			Self = this as TThis;
			Queries = queries ?? new List<ORMQuery>();
			QueryExecutor = queryExecutor ?? new BasicQueryExecutor();
		}

		protected EntityModel<TSource> GetEntityModel<TSource>()
			where TSource : new()
		{
			var entityModel = DataDomain.GetEntityModel<TSource>();
			if (entityModel == null)
				throw new System.InvalidOperationException($"Type '{typeof(TSource).FullName}' isn't modelled in the specified data domain.");
			return entityModel;
		}

		public TThis AsTransaction()
		{
			QueryExecutor = new TransactionQueryExecutor();
			return Self;
		}

		public TThis NonResultQuery(params ORMQuery[] queries)
		{
			Queries.AddRange(queries);
			return Self;
		}

		public TThis Insert<TSource>(ICollection<TSource> sources)
			where TSource : new() => Insert(sources as IEnumerable<TSource>);

		public TThis Insert<TSource>(IReadOnlyCollection<TSource> sources)
			where TSource : new() => Insert(sources as IEnumerable<TSource>);

		public TThis Insert<TSource>(IList<TSource> sources)
			where TSource : new() => Insert(sources as IEnumerable<TSource>);

		public TThis Insert<TSource>(IReadOnlyList<TSource> sources)
			where TSource : new() => Insert(sources as IEnumerable<TSource>);

		public TThis Insert<TSource>(List<TSource> sources)
			where TSource : new() => Insert(sources as IEnumerable<TSource>);

		public TThis Insert<TSource>(params TSource[] sources)
			where TSource : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().Insert(sources)
				);
			return Self;
		}

		public TThis Insert<TSource, TView>(ICollection<TView> sources)
			where TSource : new()
			where TView : new() => Insert<TSource, TView>(sources as IEnumerable<TView>);

		public TThis Insert<TSource, TView>(IReadOnlyCollection<TView> sources)
			where TSource : new()
			where TView : new() => Insert<TSource, TView>(sources as IEnumerable<TView>);

		public TThis Insert<TSource, TView>(IList<TView> sources)
			where TSource : new()
			where TView : new() => Insert<TSource, TView>(sources as IEnumerable<TView>);

		public TThis Insert<TSource, TView>(IReadOnlyList<TView> sources)
			where TSource : new()
			where TView : new() => Insert<TSource, TView>(sources as IEnumerable<TView>);

		public TThis Insert<TSource, TView>(List<TView> sources)
			where TSource : new()
			where TView : new() => Insert<TSource, TView>(sources as IEnumerable<TView>);

		public TThis Insert<TSource>(IEnumerable<TSource> sources)
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

		public TThis Insert<TSource, TView>(IEnumerable<TView> sources)
			where TSource : new()
			where TView : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().Insert<TView>(sources)
				);
			return Self;
		}

		public TThis Update<TSource>(ICollection<TSource> sources)
			where TSource : new() => Update(sources as IEnumerable<TSource>);

		public TThis Update<TSource>(IReadOnlyCollection<TSource> sources)
			where TSource : new() => Update(sources as IEnumerable<TSource>);

		public TThis Update<TSource>(IList<TSource> sources)
			where TSource : new() => Update(sources as IEnumerable<TSource>);

		public TThis Update<TSource>(IReadOnlyList<TSource> sources)
			where TSource : new() => Update(sources as IEnumerable<TSource>);

		public TThis Update<TSource>(List<TSource> sources)
			where TSource : new() => Update(sources as IEnumerable<TSource>);

		public TThis Update<TSource>(params TSource[] sources)
			where TSource : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().Update(sources)
				);
			return Self;
		}

		public TThis Update<TSource>(IEnumerable<TSource> sources)
			where TSource : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().Update(sources)
				);
			return Self;
		}

		public TThis Update<TSource, TView>(ICollection<TView> sources)
			where TSource : new()
			where TView : new() => Update<TSource, TView>(sources as IEnumerable<TView>);

		public TThis Update<TSource, TView>(IReadOnlyCollection<TView> sources)
			where TSource : new()
			where TView : new() => Update<TSource, TView>(sources as IEnumerable<TView>);

		public TThis Update<TSource, TView>(IList<TView> sources)
			where TSource : new()
			where TView : new() => Update<TSource, TView>(sources as IEnumerable<TView>);

		public TThis Update<TSource, TView>(IReadOnlyList<TView> sources)
			where TSource : new()
			where TView : new() => Update<TSource, TView>(sources as IEnumerable<TView>);

		public TThis Update<TSource, TView>(List<TView> sources)
			where TSource : new()
			where TView : new() => Update<TSource, TView>(sources as IEnumerable<TView>);

		public TThis Update<TSource, TView>(params TView[] sources)
			where TSource : new()
			where TView : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().Update<TView>(sources)
				);
			return Self;
		}

		public TThis Update<TSource, TView>(IEnumerable<TView> sources)
			where TSource : new()
			where TView : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().Update<TView>(sources)
				);
			return Self;
		}

		public TThis Delete<TSource>(ICollection<TSource> sources)
			where TSource : new() => Delete(sources as IEnumerable<TSource>);

		public TThis Delete<TSource>(IReadOnlyCollection<TSource> sources)
			where TSource : new() => Delete(sources as IEnumerable<TSource>);

		public TThis Delete<TSource>(IList<TSource> sources)
			where TSource : new() => Delete(sources as IEnumerable<TSource>);

		public TThis Delete<TSource>(IReadOnlyList<TSource> sources)
			where TSource : new() => Delete(sources as IEnumerable<TSource>);

		public TThis Delete<TSource>(List<TSource> sources)
			where TSource : new() => Delete(sources as IEnumerable<TSource>);

		public TThis Delete<TSource>(params TSource[] sources)
			where TSource : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().Delete(sources)
				);
			return Self;
		}

		public TThis Delete<TSource, TView>(ICollection<TView> sources)
			where TSource : new()
			where TView : new() => Delete<TSource, TView>(sources as IEnumerable<TView>);

		public TThis Delete<TSource, TView>(IReadOnlyCollection<TView> sources)
			where TSource : new()
			where TView : new() => Delete<TSource, TView>(sources as IEnumerable<TView>);

		public TThis Delete<TSource, TView>(IList<TView> sources)
			where TSource : new()
			where TView : new() => Delete<TSource, TView>(sources as IEnumerable<TView>);

		public TThis Delete<TSource, TView>(IReadOnlyList<TView> sources)
			where TSource : new()
			where TView : new() => Delete<TSource, TView>(sources as IEnumerable<TView>);

		public TThis Delete<TSource, TView>(List<TView> sources)
			where TSource : new()
			where TView : new() => Delete<TSource, TView>(sources as IEnumerable<TView>);

		public TThis Delete<TSource>(IEnumerable<TSource> sources)
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

		public TThis Delete<TSource, TView>(IEnumerable<TView> sources)
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

		protected List<object> ExecuteQueries(IDataProvider dataProvider)
		{
			return QueryExecutor.ExecuteQueries(Queries, dataProvider);
		}

		protected Task<List<object>> ExecuteQueriesAsync(IDataProvider dataProvider)
		{
			return QueryExecutor.ExecuteQueriesAsync(Queries, dataProvider);
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
		public QueryCollection()
		{
		}

		[Obsolete]
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
			return new QueryCollection<TSource>(DataDomain, Queries, QueryExecutor);
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
			return new QueryCollection<TView>(DataDomain, Queries, QueryExecutor);
		}

		public QueryCollection<int> SelectCount<TSource>(QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] groupBy = null)
			where TSource : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().SelectCount(where, having, groupBy)
				);
			return new QueryCollection<int>(DataDomain, Queries, QueryExecutor);
		}

		public QueryCollection<int> SelectCount<TSource, TView>(QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] groupBy = null)
			where TSource : new()
			where TView : new()
		{
			Queries.AddRange(
				GetEntityModel<TSource>().SelectCount<TView>(where, having, groupBy)
				);
			return new QueryCollection<int>(DataDomain, Queries, QueryExecutor);
		}
	}

	public class QueryCollection<TResult> : QueryCollectionBase<QueryCollection<TResult>>
	{
		public QueryCollection(DataDomain dataDomain, List<ORMQuery> queries, IORMQueryExecutor queryExecutor)
			: base(dataDomain, queries, queryExecutor)
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
			return new QueryCollection<TResult, TSource>(DataDomain, Queries, QueryExecutor);
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
			return new QueryCollection<TResult, TView>(DataDomain, Queries, QueryExecutor);
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
		public QueryCollection(DataDomain dataDomain, List<ORMQuery> queries, IORMQueryExecutor queryExecutor)
			: base(dataDomain, queries, queryExecutor)
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
