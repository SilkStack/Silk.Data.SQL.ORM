using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public abstract class ORMQuery
	{
		public Type MapToType { get; }
		public bool IsQueryResult { get; }
		public QueryExpression Query { get; }

		public ORMQuery(QueryExpression query, Type mapToType = null,
			bool isQueryResult = false)
		{
			Query = query;
			MapToType = mapToType;
			IsQueryResult = isQueryResult;
		}

		public virtual object MapResult(QueryResult queryResult)
		{
			return null;
		}

		public virtual Task<object> MapResultAsync(QueryResult queryResult)
		{
			return null;
		}
	}

	public class NoResultORMQuery : ORMQuery
	{
		public NoResultORMQuery(QueryExpression query) : base(query)
		{
		}
	}

	public class AssignAutoIncrementORMQuery : ORMQuery
	{
		private readonly DataField _field;
		private readonly IContainerReadWriter _containerReadWriter;

		public AssignAutoIncrementORMQuery(QueryExpression query, Type mapToType,
			DataField field, IContainerReadWriter containerReadWriter)
			: base(query, mapToType)
		{
			_field = field;
			_containerReadWriter = containerReadWriter;
		}

		public override object MapResult(QueryResult queryResult)
		{
			if (queryResult.Read())
			{
				if (MapToType == typeof(short))
					_field.ModelBinding.WriteValue(_containerReadWriter, queryResult.GetInt16(0));
				else if (MapToType == typeof(long))
					_field.ModelBinding.WriteValue(_containerReadWriter, queryResult.GetInt64(0));
				else if (MapToType == typeof(int))
					_field.ModelBinding.WriteValue(_containerReadWriter, queryResult.GetInt32(0));
			}
			else
				throw new Exception("Failed to get auto generated ID.");
			return null;
		}

		public override async Task<object> MapResultAsync(QueryResult queryResult)
		{
			if (await queryResult.ReadAsync().ConfigureAwait(false))
			{
				if (MapToType == typeof(short))
					_field.ModelBinding.WriteValue(_containerReadWriter, queryResult.GetInt16(0));
				else if (MapToType == typeof(long))
					_field.ModelBinding.WriteValue(_containerReadWriter, queryResult.GetInt64(0));
				else if (MapToType == typeof(int))
					_field.ModelBinding.WriteValue(_containerReadWriter, queryResult.GetInt32(0));
			}
			else
				throw new Exception("Failed to get auto generated ID.");
			return null;
		}
	}

	public abstract class MapResultORMQuery : ORMQuery
	{
		public MapResultORMQuery(QueryExpression query, Type mapToType)
			: base(query, mapToType, true)
		{
		}
	}

	public class MapResultORMQuery<TView> : MapResultORMQuery
		where TView : new()
	{
		private static TView[] _noResults = new TView[0];

		public EntityModel EntityModel { get; }

		public MapResultORMQuery(QueryExpression query, EntityModel entityModel)
			: base(query, typeof(TView))
		{
			EntityModel = entityModel;
		}

		public override object MapResult(QueryResult queryResult)
		{
			if (!queryResult.HasRows)
				return _noResults;

			var resultWriters = new List<ModelReadWriter>();
			var rowReaders = new List<ViewReadWriter>();
			var resultList = new List<TView>();

			while (queryResult.Read())
			{
				var result = new TView();
				resultList.Add(result);
				var container = new MemoryViewReadWriter(EntityModel);
				var rowReader = new RowReader(container);
				rowReader.ReadRow(queryResult);

				rowReaders.Add(container);
				resultWriters.Add(new ObjectModelReadWriter(EntityModel.Model, result));
			}

			EntityModel.MapToModelAsync(resultWriters, rowReaders)
					.ConfigureAwait(false)
					.GetAwaiter().GetResult();

			return resultList;
		}

		public override async Task<object> MapResultAsync(QueryResult queryResult)
		{
			if (!queryResult.HasRows)
				return _noResults;

			var resultWriters = new List<ModelReadWriter>();
			var rowReaders = new List<ViewReadWriter>();
			var resultList = new List<TView>();

			while (await queryResult.ReadAsync()
				.ConfigureAwait(false))
			{
				var result = new TView();
				resultList.Add(result);
				var container = new MemoryViewReadWriter(EntityModel);
				var rowReader = new RowReader(container);
				rowReader.ReadRow(queryResult);

				rowReaders.Add(container);
				resultWriters.Add(new ObjectModelReadWriter(EntityModel.Model, result));
			}

			await EntityModel.MapToModelAsync(resultWriters, rowReaders)
					.ConfigureAwait(false);

			return resultList;
		}
	}
}
