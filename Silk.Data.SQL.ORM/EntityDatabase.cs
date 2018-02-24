using System.Linq;
using System.Threading.Tasks;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.NewModelling;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.Providers;

namespace Silk.Data.SQL.ORM
{
	public class EntityDatabase<T> : IEntityDatabase<T>
		where T : new()
	{
		public EntitySchema<T> EntitySchema { get; }
		public IDataProvider DataProvider { get; }

		private QueryCollection _queryCollection = new QueryCollection();

		private InsertQueryBuilder<T> _insertQueryBuilder;
		protected InsertQueryBuilder<T> InsertQueryBuilder
		{
			get
			{
				if (_insertQueryBuilder == null)
					_insertQueryBuilder = new InsertQueryBuilder<T>(EntitySchema);
				return _insertQueryBuilder;
			}
		}

		private UpdateQueryBuilder<T> _updateQueryBuilder;
		protected UpdateQueryBuilder<T> UpdateQueryBuilder
		{
			get
			{
				if (_updateQueryBuilder == null)
					_updateQueryBuilder = new UpdateQueryBuilder<T>(EntitySchema);
				return _updateQueryBuilder;
			}
		}

		private DeleteQueryBuilder<T> _deleteQueryBuilder;
		protected DeleteQueryBuilder<T> DeleteQueryBuilder
		{
			get
			{
				if (_deleteQueryBuilder == null)
					_deleteQueryBuilder = new DeleteQueryBuilder<T>(EntitySchema);
				return _deleteQueryBuilder;
			}
		}

		public EntityDatabase(EntitySchema<T> entitySchema, IDataProvider dataProvider)
		{
			EntitySchema = entitySchema;
			DataProvider = dataProvider;
		}

		public IEntityDatabase<T> Insert(params T[] sources)
		{
			_queryCollection = _queryCollection.NonResultQuery(
				InsertQueryBuilder.CreateQuery(sources).ToArray()
				);
			return this;
		}

		public IEntityDatabase<T> Insert<TProjection>(params TProjection[] sources) where TProjection : new()
		{
			_queryCollection = _queryCollection.NonResultQuery(
				InsertQueryBuilder.CreateQuery(sources).ToArray()
				);
			return this;
		}

		public IEntityDatabase<T> AsTransaction()
		{
			throw new System.NotImplementedException();
		}

		public IEntityDatabase<T> Delete(params T[] sources)
		{
			_queryCollection = _queryCollection.NonResultQuery(
				DeleteQueryBuilder.CreateQuery(sources).ToArray()
				);
			return this;
		}

		public IEntityDatabase<T> Delete<TProjection>(params TProjection[] sources) where TProjection : new()
		{
			_queryCollection = _queryCollection.NonResultQuery(
				DeleteQueryBuilder.CreateQuery(sources).ToArray()
				);
			return this;
		}

		public IEntityDatabase<T> Delete(QueryExpression where)
		{
			_queryCollection = _queryCollection.NonResultQuery(
				DeleteQueryBuilder.CreateQuery(where: where).ToArray()
				);
			return this;
		}

		public void Execute()
		{
			_queryCollection.Execute(DataProvider);
			_queryCollection = new QueryCollection();
		}

		public async Task ExecuteAsync()
		{
			await _queryCollection.ExecuteAsync(DataProvider)
				.ConfigureAwait(false);
			_queryCollection = new QueryCollection();
		}

		public IEntityDatabase<T, T> Select(QueryExpression where = null, QueryExpression having = null, QueryExpression[] orderBy = null, QueryExpression[] groupBy = null, int? offset = null, int? limit = null)
		{
			throw new System.NotImplementedException();
		}

		public IEntityDatabase<T, TProjection> Select<TProjection>(QueryExpression where = null, QueryExpression having = null, QueryExpression[] orderBy = null, QueryExpression[] groupBy = null, int? offset = null, int? limit = null) where TProjection : new()
		{
			throw new System.NotImplementedException();
		}

		public IEntityDatabase<T, int> SelectCount(QueryExpression where = null, QueryExpression having = null, QueryExpression[] groupBy = null)
		{
			throw new System.NotImplementedException();
		}

		public IEntityDatabase<T, int> SelectCount<TProjection>(QueryExpression where = null, QueryExpression having = null, QueryExpression[] groupBy = null) where TProjection : new()
		{
			throw new System.NotImplementedException();
		}

		public IEntityDatabase<T> Update(params T[] sources)
		{
			_queryCollection = _queryCollection.NonResultQuery(
				UpdateQueryBuilder.CreateQuery(sources).ToArray()
				);
			return this;
		}

		public IEntityDatabase<T> Update<TProjection>(params TProjection[] sources) where TProjection : new()
		{
			_queryCollection = _queryCollection.NonResultQuery(
				UpdateQueryBuilder.CreateQuery(sources).ToArray()
				);
			return this;
		}
	}
}
