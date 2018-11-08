using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Queries;
using System;

namespace Silk.Data.SQL.ORM.Queries
{
	public class QueryResultReader : IModelReadWriter
	{
		public QueryResult QueryResult { get; }

		public IModel Model => throw new NotImplementedException();

		public IFieldResolver FieldResolver => throw new NotImplementedException();

		public QueryResultReader(QueryResult queryResult)
		{
			QueryResult = queryResult;
		}

		public T ReadField<T>(IFieldReference field)
		{
			if (!(field is ISchemaFieldReference<T> schemaFieldReference))
				throw new ArgumentException($"Unsupported field type '{field.GetType().FullName}'.", nameof(field));

			var ord = QueryResult.GetOrdinal(schemaFieldReference.FieldAlias);
			if (QueryResult.IsDBNull(ord))
				return default(T);

			return schemaFieldReference.ReaderFunction(QueryResult, ord);
		}

		public void WriteField<T>(IFieldReference field, T value)
		{
			throw new NotSupportedException();
		}
	}
}
