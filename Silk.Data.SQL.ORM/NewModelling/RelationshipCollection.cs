using System.Collections;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.NewModelling
{
	/// <summary>
	/// A collection of relationships.
	/// </summary>
	public class RelationshipCollection<T> : IEnumerable<T>
	{
		private readonly Dictionary<string, T> _relationships
			= new Dictionary<string, T>();

		public T this[string nameAlias]
		{
			get
			{
				return _relationships[nameAlias];
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _relationships.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
