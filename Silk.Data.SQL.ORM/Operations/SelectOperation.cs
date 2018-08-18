using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Operations.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Operations
{
	public class SelectOperation<T> : DataOperationWithResult<ICollection<T>>
	{
		private readonly QueryExpression _query;
		private readonly IProjectionMapping<T> _mapping;

		public SelectOperation(QueryExpression query, IProjectionMapping<T> mapping)
		{
			_query = query;
			_mapping = mapping;
		}

		private ICollection<T> _result;
		public override ICollection<T> Result => _result;

		public override bool CanBeBatched => true;

		public override QueryExpression GetQuery() => _query;

		public override void ProcessResult(QueryResult queryResult)
		{
			var result = new List<T>();
			var readWriter = _mapping.CreateReader(queryResult);
			while (queryResult.Read())
			{
				result.Add(_mapping.Map(readWriter));
			}
			_result = result;
		}

		public override async Task ProcessResultAsync(QueryResult queryResult)
		{
			var result = new List<T>();
			var readWriter = _mapping.CreateReader(queryResult);
			while (await queryResult.ReadAsync())
			{
				result.Add(_mapping.Map(readWriter));
			}
			_result = result;
		}
	}

	public class SelectOperation
	{
		public static SelectOperation<int> CreateCount<TEntity>(Schema.Schema schema,
			Condition where = null, Condition having = null,
			GroupBy[] groupBy = null)
		{
			return null;
		}

		public static SelectOperation<TEntity> Create<TEntity>(Schema.Schema schema,
			Condition where = null, Condition having = null,
			OrderBy[] orderBy = null, GroupBy[] groupBy = null,
			int? offset = null, int? limit = null)
			where TEntity : class
			=> Create<TEntity>(new QueryHelper<TEntity>(schema),
				where, having, orderBy, groupBy, offset, limit);

		public static SelectOperation<TEntity> Create<TEntity>(IQueryHelper<TEntity> queryHelper,
			Condition where = null, Condition having = null,
			OrderBy[] orderBy = null, GroupBy[] groupBy = null,
			int? offset = null, int? limit = null)
			where TEntity : class
			=> Create<TEntity, TEntity>(queryHelper, where, having, orderBy, groupBy, offset, limit);

		public static SelectOperation<TView> Create<TEntity, TView>(Schema.Schema schema,
			Condition where = null, Condition having = null,
			OrderBy[] orderBy = null, GroupBy[] groupBy = null,
			int? offset = null, int? limit = null)
			where TEntity : class
			where TView : class
			=> Create<TEntity, TView>(new QueryHelper<TEntity>(schema),
				where, having, orderBy, groupBy, offset, limit);

		public static SelectOperation<TView> Create<TEntity, TView>(IQueryHelper<TEntity> queryHelper,
			Condition where = null, Condition having = null,
			OrderBy[] orderBy = null, GroupBy[] groupBy = null,
			int? offset = null, int? limit = null)
			where TEntity : class
			where TView : class
		{
			var query = queryHelper.CreateQuery();
			var mapping = query.Project<TView>();

			if (where != null)
				query.AndWhere(where);

			if (having != null)
				query.AndHaving(having);

			query.Offset(offset);
			query.Limit(limit);

			var expression = query.CreateSelect();
			return new SelectOperation<TView>(expression, mapping);
		}
	}
}
