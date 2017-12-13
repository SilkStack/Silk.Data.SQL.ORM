using Silk.Data.SQL.Expressions;
using System;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class LateReadValueExpression : QueryExpression, IExtensionExpression
	{
		private readonly Func<object> _valueReadDelegate;

		public override ExpressionNodeType NodeType => ExpressionNodeType.Extension;

		public LateReadValueExpression(Func<object> valueReadDelegate)
		{
			_valueReadDelegate = valueReadDelegate;
		}

		public void Visit(QueryExpressionVisitor visitor)
		{
			visitor.Visit(QueryExpression.Value(_valueReadDelegate()));
		}
	}
}
