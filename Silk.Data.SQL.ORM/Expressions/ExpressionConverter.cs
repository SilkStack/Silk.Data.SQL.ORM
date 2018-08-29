using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class ExpressionConverter<T> where T : class
	{
		private IReadOnlyCollection<ParameterExpression> _parameters;

		public Schema.Schema Schema { get; }
		public EntitySchema<T> EntitySchema { get; }

		public ExpressionConverter(Schema.Schema schema)
		{
			Schema = schema;
			EntitySchema = schema.GetEntitySchema<T>();
			if (EntitySchema == null)
				throw new Exception("Entity isn't configured in schema.");
		}

		public ExpressionResult Convert<TResult>(Expression<Func<T, TResult>> expression)
		{
			_parameters = expression.Parameters;
			return Convert((Expression)expression);
		}

		public ExpressionResult Convert(Expression expression)
		{
			var visitor = new Visitor(Schema, EntitySchema, this, _parameters);
			var result = visitor.ConvertToQueryExpression(expression);
			return new ExpressionResult(result,
				visitor.RequiredJoins.GroupBy(q => q).Select(q => q.First()).ToArray());
		}

		private class Visitor : ExpressionVisitor
		{
			public Schema.Schema Schema { get; }
			public EntitySchema<T> EntitySchema { get; }

			public List<EntityFieldJoin> RequiredJoins { get; }
				= new List<EntityFieldJoin>();

			private readonly ExpressionConverter<T> _parent;
			private IReadOnlyCollection<ParameterExpression> _expressionParameters;
			private Stack<QueryExpression> _queryExpressionStack = new Stack<QueryExpression>();

			public Visitor(Schema.Schema schema, EntitySchema<T> entitySchema, ExpressionConverter<T> parent,
				IReadOnlyCollection<ParameterExpression> expressionParameters)
			{
				Schema = schema;
				EntitySchema = entitySchema;
				_parent = parent;
				_expressionParameters = expressionParameters;
			}

			public QueryExpression ConvertToQueryExpression(Expression node)
			{
				var expectedCount = _queryExpressionStack.Count + 1;
				Visit(node);
				if (_queryExpressionStack.Count != expectedCount)
					throw new InvalidOperationException("Unsupported expression node.");
				return _queryExpressionStack.Pop();
			}

			private void SetConversionResult(QueryExpression expression)
			{
				_queryExpressionStack.Push(expression);
			}

			protected override Expression VisitLambda<T1>(Expression<T1> node)
			{
				Visit(node.Body);
				return node;
			}

			private ValueExpression GetValueExpression(object value)
			{
				if (value is Enum)
				{
					value = (int)value;
				}
				return QueryExpression.Value(value);
			}

			protected override Expression VisitConstant(ConstantExpression node)
			{
				SetConversionResult(GetValueExpression(node.Value));
				return node;
			}

			protected override Expression VisitParameter(ParameterExpression node)
			{
				var lambdaParameter = _expressionParameters.FirstOrDefault(q => ReferenceEquals(q, node));
				if (lambdaParameter != null)
				{
					SetConversionResult(
						QueryExpression.Table(EntitySchema.EntityTable.TableName)
						);
				}
				return node;
			}

			protected override Expression VisitMethodCall(MethodCallExpression node)
			{
				var converter = Schema.GetMethodCallConverter(node.Method);
				if (converter == null)
					throw new Exception("Method call not supported.");
				var result = converter.Convert<T>(node.Method, node, _parent);
				if (result != null)
				{
					if (result.RequiredJoins != null)
						RequiredJoins.AddRange(result.RequiredJoins);
					SetConversionResult(result.QueryExpression);
				}
				return node;
			}

			protected override Expression VisitMember(MemberExpression node)
			{
				var (allExpressions, expressionPath) = FlattenExpressionTree(node);
				var lambdaParameter = _expressionParameters?.FirstOrDefault(q => ReferenceEquals(q, allExpressions[0]));
				if (lambdaParameter != null)
				{
					//  visiting a member of expression parameter, ie. a field on the entity table
					var reflectionMemberInfo = node.Member;
					var sourceExpression = ConvertToQueryExpression(allExpressions[0]);
					var entityField = EntitySchema.EntityFields
						.FirstOrDefault(q => q.ModelPath.SequenceEqual(expressionPath.Skip(1))) as EntityField;
					if (entityField != null && SqlTypeHelper.IsSqlPrimitiveType(entityField.DataType))
					{
						SetConversionResult(
							QueryExpression.Column(entityField.Columns[0].ColumnName, sourceExpression)
							);
						return node;
					}

					if (entityField == null)
					{
						//  entity field not found on model, search for the entity field through a JOIN tree
						var currentSchema = EntitySchema as EntitySchema;
						var joinChain = new List<EntityFieldJoin>();
						foreach (var pathSegment in expressionPath.Skip(1))
						{
							entityField = currentSchema.EntityFields.FirstOrDefault(q => q.ModelField.FieldName == pathSegment);
							if (entityField == null)
								throw new Exception("Couldn't resolve entity field on related object.");

							if (entityField.KeyType == KeyType.ManyToOne)
							{
								var join = currentSchema.EntityJoins.FirstOrDefault(
									q => q.EntityField == entityField
									);
								if (join == null)
									throw new Exception("Couldn't resolve JOIN for related field.");
								joinChain.Add(join);
								currentSchema = Schema.GetEntitySchema(entityField.DataType);
							}
						}

						if (entityField != null && SqlTypeHelper.IsSqlPrimitiveType(entityField.DataType))
						{
							sourceExpression = new AliasIdentifierExpression(
								joinChain.Last().TableAlias
								);
							SetConversionResult(
								QueryExpression.Column(entityField.Columns[0].ColumnName, sourceExpression)
								);
							RequiredJoins.AddRange(joinChain);
							return node;
						}
					}
				}
				else
				{
					//  simple field access (ie, acessing a variable)
					//  todo: investigate if the need for a Compile() can be removed, at least for simple cases
					var memberAccessExp = Expression.MakeMemberAccess(node.Expression, node.Member);
					var @delegate = Expression.Lambda<Func<object>>(
						Expression.Convert(memberAccessExp, typeof(object))
						);
					var value = @delegate.Compile()();
					SetConversionResult(
						GetValueExpression(value)
						);
				}

				return node;

				(Expression[] Expressions, string[] Path) FlattenExpressionTree(
					Expression expression, Expression[] expressions = null,
					string[] path = null)
				{
					if (expressions == null)
						expressions = new Expression[0];
					if (path == null)
						path = new string[0];

					expressions = new[] { expression }.Concat(expressions).ToArray();

					if (expression is MemberExpression memberExpression)
					{
						path = new[] { memberExpression.Member.Name }.Concat(path).ToArray();
						return FlattenExpressionTree(memberExpression.Expression,
							expressions, path);
					}

					if (expression is ParameterExpression parameterExpression)
					{
						path = new[] { parameterExpression.Name }.Concat(path).ToArray();
					}
					else if (expression is ConstantExpression constantExpression)
					{
						path = new[] { "const" }.Concat(path).ToArray();
					}
					else
					{
						path = new[] { "unknown" }.Concat(path).ToArray();
					}

					return (
							expressions,
							path
							);
				}
			}

			protected override Expression VisitBinary(BinaryExpression node)
			{
				var leftExpression = ConvertToQueryExpression(node.Left);
				var rightExpression = ConvertToQueryExpression(node.Right);

				switch(node.NodeType)
				{
					case ExpressionType.AddChecked:
					case ExpressionType.Add:
						SetConversionResult(
							QueryExpression.Add(leftExpression, rightExpression)
							);
						break;
					case ExpressionType.SubtractChecked:
					case ExpressionType.Subtract:
						SetConversionResult(
							QueryExpression.Subtract(leftExpression, rightExpression)
							);
						break;
					case ExpressionType.MultiplyChecked:
					case ExpressionType.Multiply:
						SetConversionResult(
							QueryExpression.Multiply(leftExpression, rightExpression)
							);
						break;
					case ExpressionType.Divide:
						SetConversionResult(
							QueryExpression.Divide(leftExpression, rightExpression)
							);
						break;


					case ExpressionType.And:
						SetConversionResult(
							new BitwiseOperationQueryExpression(leftExpression, BitwiseOperator.And, rightExpression)
							);
						break;
					case ExpressionType.ExclusiveOr:
						SetConversionResult(
							new BitwiseOperationQueryExpression(leftExpression, BitwiseOperator.ExclusiveOr, rightExpression)
							);
						break;
					case ExpressionType.Or:
						SetConversionResult(
							new BitwiseOperationQueryExpression(leftExpression, BitwiseOperator.Or, rightExpression)
							);
						break;


					case ExpressionType.AndAlso:
						SetConversionResult(
							QueryExpression.AndAlso(leftExpression, rightExpression)
							);
						break;
					case ExpressionType.OrElse:
						SetConversionResult(
							QueryExpression.OrElse(leftExpression, rightExpression)
							);
						break;


					case ExpressionType.Equal:
						SetConversionResult(
							QueryExpression.Compare(leftExpression, ComparisonOperator.AreEqual, rightExpression)
							);
						break;
					case ExpressionType.NotEqual:
						SetConversionResult(
							QueryExpression.Compare(leftExpression, ComparisonOperator.AreNotEqual, rightExpression)
							);
						break;
					case ExpressionType.GreaterThan:
						SetConversionResult(
							QueryExpression.Compare(leftExpression, ComparisonOperator.GreaterThan, rightExpression)
							);
						break;
					case ExpressionType.GreaterThanOrEqual:
						SetConversionResult(
							QueryExpression.Compare(leftExpression, ComparisonOperator.GreaterThanOrEqualTo, rightExpression)
							);
						break;
					case ExpressionType.LessThan:
						SetConversionResult(
							QueryExpression.Compare(leftExpression, ComparisonOperator.LessThan, rightExpression)
							);
						break;
					case ExpressionType.LessThanOrEqual:
						SetConversionResult(
							QueryExpression.Compare(leftExpression, ComparisonOperator.LessThanOrEqualTo, rightExpression)
							);
						break;


					default:
						throw new Exception($"Unsupported binary node type '{node.NodeType}'.");
				}

				return node;
			}
		}
	}
}
