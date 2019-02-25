using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Queries
{
	public class QueryGraphReader : IGraphReader<EntityModel, EntityField>
	{
		private readonly QueryResult _queryResult;

		public QueryGraphReader(QueryResult queryResult)
		{
			_queryResult = queryResult;
		}

		public bool CheckContainer(IFieldPath<EntityModel, EntityField> fieldPath)
			=> true;

		public bool CheckPath(IFieldPath<EntityModel, EntityField> fieldPath)
		{
			//  technically we could fetch the names for each column in the query result and check the field is actually present
			//  but do we need to in reality? the generated queries shouldn't fault
			var ord = _queryResult.GetOrdinal(fieldPath.FinalField.ProjectionAlias);
			return !_queryResult.IsDBNull(ord);
		}

		public IGraphReaderEnumerator<EntityModel, EntityField> GetEnumerator<T>(IFieldPath<EntityModel, EntityField> fieldPath)
		{
			throw new NotImplementedException();
		}

		public T Read<T>(IFieldPath<EntityModel, EntityField> fieldPath)
		{
			if (typeof(T).IsEnum)
				return (T)(object)Read<int>(fieldPath);

			var ord = _queryResult.GetOrdinal(fieldPath.FinalField.ProjectionAlias);

			if (_queryResult.IsDBNull(ord))
				return default(T);

			var typeReader = QueryTypeReaders.GetTypeReader<T>();
			if (typeReader == null)
				throw new InvalidOperationException($"Type `{typeof(T).FullName}` not supported.");
			return typeReader(_queryResult, ord);
		}
	}
}
