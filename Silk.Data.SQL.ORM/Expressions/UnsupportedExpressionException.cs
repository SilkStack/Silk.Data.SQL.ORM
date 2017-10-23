using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class UnsupportedExpressionException : Exception
	{
		public UnsupportedExpressionException(Expression expression) : base("Unsupported expression.")
		{
			Expression = expression;
		}

		public Expression Expression { get; }
	}
}
