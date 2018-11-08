using Silk.Data.SQL.Queries;

namespace Silk.Data.SQL.ORM.Schema
{
	public delegate T QueryResultReader<T>(QueryResult queryResult, int ordinal);

	public interface ISchemaFieldReference
	{
		/// <summary>
		/// Gets the alias the field has in the query result set.
		/// </summary>
		string FieldAlias { get; }
	}

	public interface ISchemaFieldReference<T> : ISchemaFieldReference
	{
		/// <summary>
		/// Gets a function that can read the field in a type-safe manner.
		/// </summary>
		QueryResultReader<T> ReaderFunction { get; }
	}
}
