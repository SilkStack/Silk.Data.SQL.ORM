using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping.Binding;
using Silk.Data.SQL.Queries;
using System.Collections.Generic;
using System.Linq;

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
		private static TypeModel<T> _typeModel = TypeModel.GetModelOf<T>();

		public ResultMapper(int resultSetCount, IEnumerable<Binding> bindings)
			: base(resultSetCount, bindings)
		{
		}

		public void Inject(T obj, QueryResult queryResult)
		{
			var objectReadWriter = new ObjectReadWriter(obj, _typeModel, typeof(T));
			var queryReader = new QueryResultReader(queryResult);
			foreach (var binding in Bindings)
			{
				binding.PerformBinding(queryReader, objectReadWriter);
			}
		}
	}
}
