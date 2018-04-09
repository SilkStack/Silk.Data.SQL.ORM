using Silk.Data.SQL.Providers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM
{
	public class EntityDatabase<T> : IDatabase<T>
		where T : class
	{
		private readonly IEntityOperations<T> _entityOperations;

		public IDataProvider DataProvider { get; }

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

		public ICollection<T> Query(Condition where = null, Condition having = null, OrderBy orderBy = null,
			GroupBy groupBy = null, int? offset = null, int? limit = null)
		{
			return DataProvider.Get(_entityOperations.CreateSelect(
				where, having, orderBy, groupBy, offset, limit
				));
		}

		public Task<ICollection<T>> QueryAsync(Condition where = null, Condition having = null, OrderBy orderBy = null, GroupBy groupBy = null, int? offset = null, int? limit = null)
		{
			return DataProvider.GetAsync(_entityOperations.CreateSelect(
				where, having, orderBy, groupBy, offset, limit
				));
		}
	}
}
