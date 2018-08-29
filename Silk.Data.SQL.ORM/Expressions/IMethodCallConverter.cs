using System.Linq.Expressions;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Expressions
{
	public interface IMethodCallConverter
	{
		ExpressionResult Convert<TEntity>(MethodInfo methodInfo, MethodCallExpression node,
			ExpressionConverter<TEntity> expressionConverter)
			where TEntity : class;
	}
}
