using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class ExpressionConverter<TSource, TView>
		where TSource : new()
		where TView : new()
	{
		public DataModel<TSource, TView> DataModel { get; }
		private ConverterVisitor _converterVisitor;

		public ExpressionConverter(DataModel<TSource, TView> dataModel)
		{
			DataModel = dataModel;
			_converterVisitor = new ConverterVisitor(dataModel);
		}

		public QueryExpression ConvertToCondition(Expression<Func<TView, bool>> expression)
		{
			var parameterName = expression.Parameters[0].Name;
			_converterVisitor.Setup(parameterName);
			_converterVisitor.Visit(expression.Body);
			return _converterVisitor.PopFromStack();
		}

		private class ConverterVisitor : ExpressionVisitor
		{
			private string _parameterName;
			private readonly Stack<QueryExpression> _expressionStack = new Stack<QueryExpression>();
			private readonly DataModel<TSource, TView> _dataModel;

			public ConverterVisitor(DataModel<TSource, TView> dataModel)
			{
				_dataModel = dataModel;
			}

			public void Setup(string parameterName)
			{
				_parameterName = parameterName;
				_expressionStack.Clear();
			}

			private QueryExpression ConvertExpression(Expression expression)
			{
				var stackSize = _expressionStack.Count;
				Visit(expression);
				if (stackSize == _expressionStack.Count)
					throw new UnsupportedExpressionException(expression);
				return PopFromStack();
			}

			public QueryExpression PopFromStack()
			{
				return _expressionStack.Pop();
			}

			private void PushOntoStack(QueryExpression queryExpression)
			{
				_expressionStack.Push(queryExpression);
			}

			protected override Expression VisitConstant(ConstantExpression node)
			{
				PushOntoStack(QueryExpression.Value(node.Value));
				return node;
			}

			protected override Expression VisitMember(MemberExpression node)
			{
				var expressionParameter = GetParameterExpression(node);
				if (expressionParameter != null)
				{
					//  member is based from a parameter to a lambda expression
					PushOntoStack(QueryExpression.Column(
						_dataModel.Fields.First(q => q.Name == node.Member.Name)
							.Storage.ColumnName
						));
				}
				else
				{
					//  member is based from a hoisted parameter
					var memberAccessExp = Expression.MakeMemberAccess(node.Expression, node.Member);
					var @delegate = Expression.Lambda<Func<object>>(Expression.Convert(memberAccessExp, typeof(object)));
					var value = @delegate.Compile()();
					PushOntoStack(QueryExpression.Value(value));
				}
				return node;
			}

			private static ParameterExpression GetParameterExpression(MemberExpression memberExpression)
			{
				if (memberExpression.Expression == null)
					return null;
				if (memberExpression.Expression is ParameterExpression parameterExpr)
					return parameterExpr;
				if (memberExpression.Expression is MemberExpression subMemberExpression)
					return GetParameterExpression(subMemberExpression);
				return null;
			}

			protected override Expression VisitBinary(BinaryExpression node)
			{
				var left = ConvertExpression(node.Left);
				var right = ConvertExpression(node.Right);
				switch (node.NodeType)
				{
					case ExpressionType.AndAlso:
						PushOntoStack(
							QueryExpression.AndAlso(left, right)
						);
						break;
					case ExpressionType.OrElse:
						PushOntoStack(
							QueryExpression.OrElse(left, right)
						);
						break;
					case ExpressionType.Equal:
					case ExpressionType.NotEqual:
					case ExpressionType.GreaterThan:
					case ExpressionType.GreaterThanOrEqual:
					case ExpressionType.LessThan:
					case ExpressionType.LessThanOrEqual:
						PushOntoStack(
							QueryExpression.Compare(left, GetComparisonOperator(node.NodeType), right)
						);
						break;

				}
				return node;
			}

			private static ComparisonOperator GetComparisonOperator(ExpressionType linqExpressionType)
			{
				switch (linqExpressionType)
				{
					case ExpressionType.Equal: return ComparisonOperator.AreEqual;
					case ExpressionType.NotEqual: return ComparisonOperator.AreNotEqual;
					case ExpressionType.GreaterThan: return ComparisonOperator.GreaterThan;
					case ExpressionType.GreaterThanOrEqual: return ComparisonOperator.GreaterThanOrEqualTo;
					case ExpressionType.LessThan: return ComparisonOperator.LessThan;
					case ExpressionType.LessThanOrEqual: return ComparisonOperator.LessThanOrEqualTo;
				}
				throw new ArgumentException("Unknown expression type.", nameof(linqExpressionType));
			}
		}
	}
}
