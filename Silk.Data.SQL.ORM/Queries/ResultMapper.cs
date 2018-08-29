using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping.Binding;
using Silk.Data.SQL.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	/// <summary>
	/// Takes the query result from a built query and maps it to object instances.
	/// </summary>
	public abstract class ResultMapper
	{
		public int ResultSetCount { get; }

		public ResultMapper(int resultSetCount)
		{
			ResultSetCount = resultSetCount;
		}
	}

	public class ValueResultMapper<T> : ResultMapper
	{
		private readonly string[] _aliasPath;

		public ValueResultMapper(int resultSetCount, string aliasName)
			: base(resultSetCount)
		{
			_aliasPath = new[] { aliasName };
		}

		public T Read(QueryResult queryResult)
		{
			var queryReader = new QueryResultReader(queryResult);
			return queryReader.ReadField<T>(_aliasPath, 0);
		}

		public ICollection<T> ReadResultSet(QueryResult queryResult)
		{
			var result = new List<T>();
			var queryReader = new QueryResultReader(queryResult);
			while (queryResult.Read())
			{
				result.Add(queryReader.ReadField<T>(_aliasPath, 0));
			}
			return result;
		}

		public async Task<ICollection<T>> ReadResultSetAsync(QueryResult queryResult)
		{
			var result = new List<T>();
			var queryReader = new QueryResultReader(queryResult);
			while (await queryResult.ReadAsync())
			{
				result.Add(queryReader.ReadField<T>(_aliasPath, 0));
			}
			return result;
		}

		public ICollection<T> ReadAll(QueryResult queryResult)
		{
			var result = new List<T>();
			var queryReader = new QueryResultReader(queryResult);
			for (var i = 0; i < ResultSetCount; i++)
			{
				while (queryResult.Read())
				{
					result.Add(queryReader.ReadField<T>(_aliasPath, 0));
				}

				if (!queryResult.NextResult())
					break;
			}
			return result;
		}

		public async Task<ICollection<T>> ReadAllAsync(QueryResult queryResult)
		{
			var result = new List<T>();
			var queryReader = new QueryResultReader(queryResult);
			for (var i = 0; i < ResultSetCount; i++)
			{
				while (await queryResult.ReadAsync())
				{
					result.Add(queryReader.ReadField<T>(_aliasPath, 0));
				}

				if (!await queryResult.NextResultAsync())
					break;
			}
			return result;
		}
	}

	public class ObjectResultMapper<T> : ResultMapper
	{
		private readonly static string[] _selfPath = new[] { "." };
		private readonly static TypeModel<T> _typeModel = TypeModel.GetModelOf<T>();

		private readonly Binding[] _bindings;

		public ObjectResultMapper(int resultSetCount, IEnumerable<Binding> bindings)
			: base(resultSetCount)
		{
			_bindings = bindings.ToArray();
		}

		public void Inject(T obj, QueryResult queryResult)
		{
			var objectReadWriter = new ObjectReadWriter(obj, _typeModel, typeof(T));
			var queryReader = new QueryResultReader(queryResult);
			Bind(queryReader, objectReadWriter);
		}

		public T Map(QueryResult queryResult)
		{
			var objectReadWriter = new ObjectReadWriter(null, _typeModel, typeof(T));
			var queryReader = new QueryResultReader(queryResult);
			Bind(queryReader, objectReadWriter);
			return objectReadWriter.ReadField<T>(_selfPath, 0);
		}

		public ICollection<T> MapResultSet(QueryResult queryResult)
		{
			var queryReader = new QueryResultReader(queryResult);
			var result = new List<T>();
			while (queryResult.Read())
			{
				var objectReadWriter = new ObjectReadWriter(null, _typeModel, typeof(T));
				Bind(queryReader, objectReadWriter);
				result.Add(objectReadWriter.ReadField<T>(_selfPath, 0));
			}
			return result;
		}

		public async Task<ICollection<T>> MapResultSetAsync(QueryResult queryResult)
		{
			var queryReader = new QueryResultReader(queryResult);
			var result = new List<T>();
			while (await queryResult.ReadAsync())
			{
				var objectReadWriter = new ObjectReadWriter(null, _typeModel, typeof(T));
				Bind(queryReader, objectReadWriter);
				result.Add(objectReadWriter.ReadField<T>(_selfPath, 0));
			}
			return result;
		}

		public ICollection<T> MapAll(QueryResult queryResult)
		{
			var queryReader = new QueryResultReader(queryResult);
			var result = new List<T>();
			for (var i = 0; i < ResultSetCount; i++)
			{
				while (queryResult.Read())
				{
					var objectReadWriter = new ObjectReadWriter(null, _typeModel, typeof(T));
					Bind(queryReader, objectReadWriter);
					result.Add(objectReadWriter.ReadField<T>(_selfPath, 0));
				}

				if (!queryResult.NextResult())
					break;
			}
			return result;
		}

		public async Task<ICollection<T>> MapAllAsync(QueryResult queryResult)
		{
			var queryReader = new QueryResultReader(queryResult);
			var result = new List<T>();
			for (var i = 0; i < ResultSetCount; i++)
			{
				while (await queryResult.ReadAsync())
				{
					var objectReadWriter = new ObjectReadWriter(null, _typeModel, typeof(T));
					Bind(queryReader, objectReadWriter);
					result.Add(objectReadWriter.ReadField<T>(_selfPath, 0));
				}

				if (!await queryResult.NextResultAsync())
					break;
			}
			return result;
		}

		private void Bind(IModelReadWriter from, IModelReadWriter to)
		{
			foreach (var binding in _bindings)
			{
				binding.PerformBinding(from, to);
			}
		}
	}
}
