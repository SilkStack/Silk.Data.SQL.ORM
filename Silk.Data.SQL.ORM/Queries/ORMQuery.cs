using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.Queries;
using System;
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
}
