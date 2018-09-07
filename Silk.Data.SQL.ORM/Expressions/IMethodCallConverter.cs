using System.Linq.Expressions;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Expressions
{
	public interface IMethodCallConverter
	{
		ExpressionResult Convert(MethodInfo methodInfo, MethodCallExpression node,
			ExpressionConverter expressionConverter);
	}
}
