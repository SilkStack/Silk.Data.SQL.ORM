﻿using Silk.Data.SQL.Expressions;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Queries
{
	public class CompositeQueryExpression : QueryExpression, IExtensionExpression
	{
		public override ExpressionNodeType NodeType => ExpressionNodeType.Extension;

		public List<QueryExpression> Queries { get; } = new List<QueryExpression>();

		public CompositeQueryExpression(IEnumerable<QueryExpression> queries)
		{
			Queries.AddRange(queries);
		}

		public CompositeQueryExpression(params QueryExpression[] queries)
		{
			Queries.AddRange(queries);
		}

		public void Visit(QueryExpressionVisitor visitor)
		{
			foreach (var query in Queries)
			{
				visitor.Visit(query);
			}
		}
	}
}
