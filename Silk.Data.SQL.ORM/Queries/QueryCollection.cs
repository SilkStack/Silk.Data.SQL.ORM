using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public abstract class QueryCollection
	{
		protected List<QueryWithDelegate> Queries { get; } = new List<QueryWithDelegate>();
	}

	public class ExecutableQueryCollection : QueryCollection
	{
		public ExecutableQueryCollection(IEnumerable<QueryWithDelegate> queryExpressions)
		{
			Queries.AddRange(queryExpressions);
		}

		public ExecutableQueryCollection(params QueryWithDelegate[] queryExpressions)
		{
			Queries.AddRange(queryExpressions);
		}

		public ExecutableQueryCollection AsTransaction()
		{
			return new ExecutableQueryCollection(new QueryWithDelegate(
				QueryExpression.Transaction(Queries.Select(q => q.Query)),
				queryResult =>
				{
					foreach (var query in Queries)
					{
						if (query.Query is SelectExpression)
						{
							if (!queryResult.NextResult())
								throw new Exception("Failed to move to query result.");
							query.Delegate(queryResult);
						}
					}
				},
				async queryResult =>
				{
					foreach (var query in Queries)
					{
						if (query.Query is SelectExpression)
						{
							if (!await queryResult.NextResultAsync().ConfigureAwait(false))
								throw new Exception("Failed to move to query result.");
							await query.AsyncDelegate(queryResult).ConfigureAwait(false);
						}
					}
				}));
		}

		public void Execute(IDataProvider dataProvider)
		{
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
					}
				}
			}
		}

		public async Task ExecuteAsync(IDataProvider dataProvider)
		{
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
					}
				}
			}
		}
	}

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
	}
}
