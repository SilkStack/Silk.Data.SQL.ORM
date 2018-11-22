using Silk.Data.Modelling;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Schema
{
	public delegate T QueryResultReader<T>(QueryResult queryResult, int ordinal);

	public interface ISchemaFieldReference : IFieldReference
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

	public class SchemaFieldReference<T> : ISchemaFieldReference<T>
	{
		public QueryResultReader<T> ReaderFunction { get; }

		public string FieldAlias { get; }

		public IField Field => throw new System.NotSupportedException();

		public IModel Model => throw new System.NotSupportedException();

		public SchemaFieldReference(string alias, QueryResultReader<T> readerFunction)
		{
			FieldAlias = alias;
			ReaderFunction = readerFunction;
		}

		private static readonly Dictionary<Type, Delegate> _typeReaders =
			new Dictionary<Type, Delegate>()
			{
				{ typeof(bool), new QueryResultReader<bool>((q,o) => q.GetBoolean(o)) },
				{ typeof(byte), new QueryResultReader<byte>((q,o) => q.GetByte(o)) },
				{ typeof(short), new QueryResultReader<short>((q,o) => q.GetInt16(o)) },
				{ typeof(int), new QueryResultReader<int>((q,o) => q.GetInt32(o)) },
				{ typeof(long), new QueryResultReader<long>((q,o) => q.GetInt64(o)) },
				{ typeof(float), new QueryResultReader<float>((q,o) => q.GetFloat(o)) },
				{ typeof(double), new QueryResultReader<double>((q,o) => q.GetDouble(o)) },
				{ typeof(decimal), new QueryResultReader<decimal>((q,o) => q.GetDecimal(o)) },
				{ typeof(string), new QueryResultReader<string>((q,o) => q.GetString(o)) },
				{ typeof(Guid), new QueryResultReader<Guid>((q,o) => q.GetGuid(o)) },
				{ typeof(DateTime), new QueryResultReader<DateTime>((q,o) => q.GetDateTime(o)) },

				{ typeof(bool?), new QueryResultReader<bool?>((q,o) => q.GetBoolean(o)) },
				{ typeof(byte?), new QueryResultReader<byte?>((q,o) => q.GetByte(o)) },
				{ typeof(short?), new QueryResultReader<short?>((q,o) => q.GetInt16(o)) },
				{ typeof(int?), new QueryResultReader<int?>((q,o) => q.GetInt32(o)) },
				{ typeof(long?), new QueryResultReader<long?>((q,o) => q.GetInt64(o)) },
				{ typeof(float?), new QueryResultReader<float?>((q,o) => q.GetFloat(o)) },
				{ typeof(double?), new QueryResultReader<double?>((q,o) => q.GetDouble(o)) },
				{ typeof(decimal?), new QueryResultReader<decimal?>((q,o) => q.GetDecimal(o)) },
				{ typeof(Guid?), new QueryResultReader<Guid?>((q,o) => q.GetGuid(o)) },
				{ typeof(DateTime?), new QueryResultReader<DateTime?>((q,o) => q.GetDateTime(o)) }
			};

		public static SchemaFieldReference<T> Create(ISchemaField schemaField)
		{
			Delegate readerDelegate;
			if (typeof(T).IsEnum)
			{
				readerDelegate = GetEnumReader();
			}
			else
			{
				if (!_typeReaders.TryGetValue(typeof(T), out readerDelegate))
					throw new InvalidOperationException("Data type not supported.");
			}
			return new SchemaFieldReference<T>(schemaField.AliasName, readerDelegate as QueryResultReader<T>);
		}

		private static QueryResultReader<T> GetEnumReader()
		{
			var @delegate = _typeReaders[typeof(int)] as QueryResultReader<int>;
			return new QueryResultReader<T>((q,o) => (T)(object)@delegate(q,o));
		}
	}
}
