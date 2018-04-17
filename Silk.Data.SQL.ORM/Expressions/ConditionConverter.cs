using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class ConditionConverter<T>
	{
		public QueryExpression ConvertToCondition(Expression<Func<T, bool>> expression, EntityModel<T> entityModel)
		{
			var visitor = new Visitor(entityModel);
			visitor.Visit(expression);
			return visitor.Expression;
		}

		private class Visitor : ExpressionVisitor
		{
			private readonly EntityModel<T> _entityModel;
			private IReadOnlyCollection<ParameterExpression> _expressionParameters;
			private Stack<QueryExpression> _queryExpressionStack = new Stack<QueryExpression>();

			public QueryExpression Expression { get; private set; }

			public Visitor(EntityModel<T> entityModel)
			{
				_entityModel = entityModel;
			}

			private QueryExpression ConvertToQueryExpression(Expression node)
			{
				var expectedCount = _queryExpressionStack.Count + 1;
				Visit(node);
				if (_queryExpressionStack.Count != expectedCount)
					throw new InvalidOperationException("Unsupported expression node.");
				return _queryExpressionStack.Pop();
			}

			protected override Expression VisitLambda<TLambda>(Expression<TLambda> node)
			{
				if (_expressionParameters == null)
				{
					_expressionParameters = node.Parameters;
					Expression = ConvertToQueryExpression(node.Body);
					return node;
				}

				return base.VisitLambda(node);
			}

			protected override Expression VisitBinary(BinaryExpression node)
			{
				var leftExpression = ConvertToQueryExpression(node.Left);
				var rightExpression = ConvertToQueryExpression(node.Right);

				switch (node.NodeType)
				{
					case ExpressionType.AndAlso:
						_queryExpressionStack.Push(
							QueryExpression.AndAlso(leftExpression, rightExpression)
							);
						break;
					case ExpressionType.OrElse:
						_queryExpressionStack.Push(
							QueryExpression.OrElse(leftExpression, rightExpression)
							);
						break;
					case ExpressionType.Equal:
					case ExpressionType.NotEqual:
					case ExpressionType.GreaterThan:
					case ExpressionType.GreaterThanOrEqual:
					case ExpressionType.LessThan:
					case ExpressionType.LessThanOrEqual:
						_queryExpressionStack.Push(
							QueryExpression.Compare(leftExpression, GetComparisonOperator(node.NodeType), rightExpression)
						);
						break;
					default:
						throw new Exception("Unhandled BinaryNode type.");
				}

				return node;
			}

			protected override Expression VisitConstant(ConstantExpression node)
			{
				_queryExpressionStack.Push(QueryExpression.Value(node.Value));
				return node;
			}

			protected override Expression VisitMember(MemberExpression node)
			{
				var lambdaParameter = _expressionParameters.FirstOrDefault(q => ReferenceEquals(q, node.Expression));
				if (lambdaParameter != null)
				{
					var reflectionMemberInfo = node.Member;
					var queryExpression = ConvertToQueryExpression(node.Expression);
					var entityField = _entityModel.Fields.FirstOrDefault(q => q.FieldName == reflectionMemberInfo.Name);
					if (entityField is IValueField valueField)
					{
						_queryExpressionStack.Push(
							QueryExpression.Column(valueField.Column.ColumnName, queryExpression)
							);
					}
				}
				else
				{
					var memberAccessExp = System.Linq.Expressions.Expression.MakeMemberAccess(node.Expression, node.Member);
					var @delegate = System.Linq.Expressions.Expression.Lambda<Func<object>>(
						System.Linq.Expressions.Expression.Convert(memberAccessExp, typeof(object))
						);
					var value = @delegate.Compile()();
					_queryExpressionStack.Push(
						QueryExpression.Value(value)
						);
				}
				return node;
			}

			protected override Expression VisitParameter(ParameterExpression node)
			{
				var lambdaParameter = _expressionParameters.FirstOrDefault(q => ReferenceEquals(q, node));
				if (lambdaParameter != null)
				{
					_queryExpressionStack.Push(
						QueryExpression.Table(_entityModel.EntityTable.TableName)
						);
				}
				return node;
			}

			protected override Expression VisitMethodCall(MethodCallExpression node)
			{
				//  todo: add support for QueryExpression dbfunctions here?
				var value = System.Linq.Expressions.Expression.Lambda(node).Compile().DynamicInvoke();
				_queryExpressionStack.Push(QueryExpression.Value(value));
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
