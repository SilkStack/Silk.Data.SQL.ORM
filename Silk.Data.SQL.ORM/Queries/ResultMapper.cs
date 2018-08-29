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

		protected Binding[] Bindings { get; }

		public ResultMapper(int resultSetCount, IEnumerable<Binding> bindings)
		{
			ResultSetCount = resultSetCount;
			Bindings = bindings.ToArray();
		}
	}

	public class ResultMapper<T> : ResultMapper
	{
		private readonly static string[] _selfPath = new[] { "." };
		private readonly static TypeModel<T> _typeModel = TypeModel.GetModelOf<T>();

		public ResultMapper(int resultSetCount, IEnumerable<Binding> bindings)
			: base(resultSetCount, bindings)
		{
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
				if (!queryResult.NextResult())
					break;

				while (queryResult.Read())
				{
					var objectReadWriter = new ObjectReadWriter(null, _typeModel, typeof(T));
					Bind(queryReader, objectReadWriter);
					result.Add(objectReadWriter.ReadField<T>(_selfPath, 0));
				}
			}
			return result;
		}

		public async Task<ICollection<T>> MapAllAsync(QueryResult queryResult)
		{
			var queryReader = new QueryResultReader(queryResult);
			var result = new List<T>();
			for (var i = 0; i < ResultSetCount; i++)
			{
				if (!await queryResult.NextResultAsync())
					break;

				while (await queryResult.ReadAsync())
				{
					var objectReadWriter = new ObjectReadWriter(null, _typeModel, typeof(T));
					Bind(queryReader, objectReadWriter);
					result.Add(objectReadWriter.ReadField<T>(_selfPath, 0));
				}
			}
			return result;
		}

		private void Bind(IModelReadWriter from, IModelReadWriter to)
		{
			foreach (var binding in Bindings)
			{
				binding.PerformBinding(from, to);
			}
		}
	}
}
