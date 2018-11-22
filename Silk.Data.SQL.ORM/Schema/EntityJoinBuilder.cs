using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silk.Data.SQL.ORM.Schema
{
	public class EntityJoinBuilder
	{
		private EntityFieldJoin _builtJoin;

		public string TableName { get; }
		public string TableAlias { get; }
		public string SourceName { get; }
		public string[] LeftColumns { get; }
		public string[] RightColumns { get; }
		public EntityJoinBuilder[] DependencyJoins { get; }
		public string[] ModelPath { get; }

		public EntityJoinBuilder(string tableName, string tableAlias,
			string sourceName, IEnumerable<string> leftColumns, IEnumerable<string> rightColumns,
			EntityJoinBuilder[] dependencyJoins, string[] modelPath)
		{
			TableName = tableName;
			TableAlias = tableAlias;
			SourceName = sourceName;
			LeftColumns = leftColumns.ToArray();
			RightColumns = rightColumns.ToArray();
			DependencyJoins = dependencyJoins;
			ModelPath = modelPath;
		}

		public EntityFieldJoin Build()
		{
			if (_builtJoin == null)
				_builtJoin = new EntityFieldJoin(
					TableName, TableAlias, SourceName, LeftColumns, RightColumns, null,
					DependencyJoins.Select(q => q.Build()).ToArray()
					);
			return _builtJoin;
		}
	}
}
