using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.Providers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM
{
	public class EntityDatabase<T> : IDatabase<T>
		where T : class
	{
		private readonly IEntityOperations<T> _entityOperations;

		public IDataProvider DataProvider { get; }

		public EntityModel<T> EntityModel => _entityOperations.EntityModel;

		public EntityDatabase(IEntityOperations<T> entityOperations, IDataProvider dataProvider)
		{
			_entityOperations = entityOperations;
			DataProvider = dataProvider;
		}

		public EntityDatabase(Schema.Schema schema, IDataProvider dataProvider) :
			this(new EntityOperations<T>(schema), dataProvider)
		{
		}

		public void Insert(IEnumerable<T> entities)
		{
			DataProvider.Insert(_entityOperations.CreateInsert(entities));
		}

		public void Insert<TView>(IEnumerable<TView> entities)
			where TView : class
		{
			DataProvider.Insert(_entityOperations.CreateInsert(entities));
		}

		public Task InsertAsync(IEnumerable<T> entities)
		{
			return DataProvider.InsertAsync(_entityOperations.CreateInsert(entities));
		}

		public Task InsertAsync<TView>(IEnumerable<TView> entities)
			where TView : class
		{
			return DataProvider.InsertAsync(_entityOperations.CreateInsert(entities));
		}

		public ICollection<T> Query(Condition where = null, Condition having = null, OrderBy[] orderBy = null,
			GroupBy[] groupBy = null, int? offset = null, int? limit = null)
		{
			return DataProvider.Get(_entityOperations.CreateSelect(
				where, having, orderBy, groupBy, offset, limit
				));
		}

		public Task<ICollection<T>> QueryAsync(Condition where = null, Condition having = null, OrderBy[] orderBy = null, GroupBy[] groupBy = null, int? offset = null, int? limit = null)
		{
			return DataProvider.GetAsync(_entityOperations.CreateSelect(
				where, having, orderBy, groupBy, offset, limit
				));
		}

		public ICollection<TView> Query<TView>(Condition where = null, Condition having = null, OrderBy[] orderBy = null, GroupBy[] groupBy = null, int? offset = null, int? limit = null) where TView : class
		{
			return DataProvider.Get(_entityOperations.CreateSelect<TView>(
				where, having, orderBy, groupBy, offset, limit
				));
		}

		public Task<ICollection<TView>> QueryAsync<TView>(Condition where = null, Condition having = null, OrderBy[] orderBy = null, GroupBy[] groupBy = null, int? offset = null, int? limit = null) where TView : class
		{
			return DataProvider.GetAsync(_entityOperations.CreateSelect<TView>(
				where, having, orderBy, groupBy, offset, limit
				));
		}

		public void Delete(IEnumerable<T> entities)
		{
			DataProvider.ExecuteNonReader(_entityOperations.CreateDelete(
				entities
				));
		}

		public Task DeleteAsync(IEnumerable<T> entities)
		{
			return DataProvider.ExecuteNonReaderAsync(_entityOperations.CreateDelete(
				entities
				));
		}

		public void Delete(Condition where)
		{
			DataProvider.ExecuteNonReader(_entityOperations.CreateDelete(where));
		}

		public Task DeleteAsync(Condition where)
		{
			return DataProvider.ExecuteNonReaderAsync(_entityOperations.CreateDelete(where));
		}

		public void Update(IEnumerable<T> entities)
		{
			DataProvider.ExecuteNonReader(_entityOperations.CreateUpdate(entities));
		}

		public Task UpdateAsync(IEnumerable<T> entities)
		{
			return DataProvider.ExecuteNonReaderAsync(_entityOperations.CreateUpdate(entities));
		}

		public void Update<TView>(TView view, Condition where)
			where TView : class
		{
			DataProvider.ExecuteNonReader(_entityOperations.CreateUpdate(view, where));
		}

		public Task UpdateAsync<TView>(TView view, Condition where)
			where TView : class
		{
			return DataProvider.ExecuteNonReaderAsync(_entityOperations.CreateUpdate(view, where));
		}

		public int Count(Condition where = null, Condition having = null, GroupBy[] groupBy = null)
		{
			return DataProvider.GetSingle(_entityOperations.CreateCount(where, having, groupBy));
		}

		public Task<int> CountAsync(Condition where = null, Condition having = null, GroupBy[] groupBy = null)
		{
			return DataProvider.GetSingleAsync(_entityOperations.CreateCount(where, having, groupBy));
		}
	}
}
