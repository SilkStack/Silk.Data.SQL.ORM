using Silk.Data.SQL.ORM.Schema;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Expressions
{
	public static class MethodHelper
	{
		public static IEnumerable<EntityFieldJoin> ConcatJoins(params ExpressionResult[] expressionResults)
		{
			foreach (var result in expressionResults)
			{
				if (result == null || result.RequiredJoins == null || result.RequiredJoins.Length == 0)
					continue;
				foreach (var join in result.RequiredJoins)
					yield return join;
			}
		}
	}
}
