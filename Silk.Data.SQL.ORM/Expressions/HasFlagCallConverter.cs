using Silk.Data.SQL.Expressions;
using System.Linq.Expressions;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class HasFlagCallConverter : IMethodCallConverter
	{
		public ExpressionResult Convert<TEntity>(MethodInfo methodInfo, MethodCallExpression node,
			ExpressionConverter<TEntity> expressionConverter)
			where TEntity : class
		{
			if (node.Arguments.Count != 1)
				throw new System.Exception("Incorrect number of arguments to HasFlag");

			var enumValueConvertExpression = node.Arguments[0] as UnaryExpression;
			var enumValueConstantExpression = enumValueConvertExpression.Operand;
			var convertedArgument = expressionConverter.Convert(enumValueConstantExpression);

			var calledOn = expressionConverter.Convert(node.Object);
			
			return new ExpressionResult(
				QueryExpression.Compare(
					new BitwiseOperationQueryExpression(calledOn.QueryExpression, BitwiseOperator.And, convertedArgument.QueryExpression),
					ComparisonOperator.AreEqual, convertedArgument.QueryExpression)
				);
		}
	}
}
