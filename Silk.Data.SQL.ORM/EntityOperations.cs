using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM
{
	public class EntityOperations<T> : IEntityOperations<T>
		where T : class
	{
		private readonly EntityModel<T> _entityModel;

		public EntityOperations(Schema.Schema schema)
		{
			_entityModel = schema.GetEntityModel<T>();

			if (_entityModel == null)
				throw new InvalidOperationException($"Knowledge of entity type {typeof(T).FullName} not present in provided schema.");
		}

		public InsertOperation CreateInsert(IEnumerable<T> entities)
		{
			return CreateInsert(entities.ToArray());
		}

		public InsertOperation CreateInsert(params T[] entities)
		{
			return InsertOperation.Create<T>(_entityModel, entities);
		}

		public InsertOperation CreateInsert<TView>(IEnumerable<TView> entities) where TView : class
		{
			return CreateInsert<TView>(entities.ToArray());
		}

		public InsertOperation CreateInsert<TView>(params TView[] entities) where TView : class
		{
			return InsertOperation.Create<T, TView>(_entityModel, entities);
		}

		public SelectOperation<T> CreateSelect(Condition where = null, Condition having = null, OrderBy orderBy = null, GroupBy groupBy = null, int? offset = null, int? limit = null)
		{
			return SelectOperation.Create<T>(_entityModel, where, having, orderBy, groupBy, offset, limit);
		}
	}
}
