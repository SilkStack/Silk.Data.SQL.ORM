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
		public EntityModel<T> EntityModel { get; }

		public EntityOperations(Schema.Schema schema)
		{
			EntityModel = schema.GetEntityModel<T>();

			if (EntityModel == null)
				throw new InvalidOperationException($"Knowledge of entity type {typeof(T).FullName} not present in provided schema.");
		}

		public InsertOperation CreateInsert(IEnumerable<T> entities)
		{
			return CreateInsert(entities.ToArray());
		}

		public InsertOperation CreateInsert(params T[] entities)
		{
			return InsertOperation.Create<T>(EntityModel, entities);
		}

		public InsertOperation CreateInsert<TView>(IEnumerable<TView> entities) where TView : class
		{
			return CreateInsert<TView>(entities.ToArray());
		}

		public InsertOperation CreateInsert<TView>(params TView[] entities) where TView : class
		{
			return InsertOperation.Create<T, TView>(EntityModel, entities);
		}

		public SelectOperation<T> CreateSelect(Condition where = null, Condition having = null, OrderBy orderBy = null, GroupBy groupBy = null, int? offset = null, int? limit = null)
		{
			return SelectOperation.Create<T>(EntityModel, where, having, orderBy, groupBy, offset, limit);
		}

		public SelectOperation<TView> CreateSelect<TView>(Condition where = null, Condition having = null, OrderBy orderBy = null, GroupBy groupBy = null, int? offset = null, int? limit = null) where TView : class
		{
			return SelectOperation.Create<T, TView>(EntityModel, where, having, orderBy, groupBy, offset, limit);
		}

		public DeleteOperation CreateDelete(IEnumerable<T> entities)
		{
			return CreateDelete(entities.ToArray());
		}

		public DeleteOperation CreateDelete(params T[] entities)
		{
			return DeleteOperation.Create<T>(EntityModel, entities);
		}

		public DeleteOperation CreateDelete(Condition where)
		{
			return DeleteOperation.Create(EntityModel, where);
		}

		public UpdateOperation CreateUpdate(IEnumerable<T> entities)
		{
			return CreateUpdate(entities.ToArray());
		}

		public UpdateOperation CreateUpdate(params T[] entities)
		{
			return UpdateOperation.Create<T>(EntityModel, entities);
		}

		public UpdateOperation CreateUpdate<TView>(TView view, Condition condition)
			where TView : class
		{
			return UpdateOperation.Create(EntityModel, view, condition);
		}

		public SelectOperation<int> CreateCount(Condition where = null, Condition having = null, GroupBy groupBy = null)
		{
			return SelectOperation.CreateCount(EntityModel, where, having, groupBy);
		}
	}
}
