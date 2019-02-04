using Silk.Data.SQL.ORM.Schema;
using System.Collections;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Queries
{
	internal class JoinCollection : IEnumerable<EntityFieldJoin>
	{
		private List<EntityFieldJoin> _joins = new List<EntityFieldJoin>();

		public IEnumerator<EntityFieldJoin> GetEnumerator()
			=> _joins.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		public void AddJoins(EntityFieldJoin[] joins)
		{
			if (joins == null || joins.Length < 1)
				return;
			foreach (var join in joins)
			{
				AddJoin(join);
			}
		}

		public void AddJoin(EntityFieldJoin join)
		{
			if (join == null || _joins.Contains(join))
				return;
			_joins.Add(join);
			AddJoins(join.DependencyJoins);
		}
	}
}
