using Silk.Data.SQL.ORM.Schema;
using System.Collections;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Queries
{
	internal class JoinCollection : IEnumerable<Join>
	{
		private List<Join> _joins = new List<Join>();

		public IEnumerator<Join> GetEnumerator()
			=> _joins.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		public void AddJoins(params Join[] joins)
		{
			foreach (var join in joins)
				AddJoin(join);
		}

		public void AddJoin(Join join)
		{
			if (join == null || _joins.Contains(join))
				return;
			_joins.Add(join);

			var dependencyJoin = join.Left as Join;
			if (dependencyJoin != null)
				AddJoin(dependencyJoin);
			dependencyJoin = join.Right as Join;
			if (dependencyJoin != null)
				AddJoin(dependencyJoin);
		}
	}
}
