using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Expressions
{
	public abstract class ExpressionConverter
	{
		public Schema.Schema Schema { get; }

		protected Dictionary<ParameterExpression, EntityModel> Parameters { get; set; }

		public ExpressionConverter(Schema.Schema schema)
		{
			Schema = schema;
		}

		public ExpressionResult Convert(Expression expression)
		{
			var visitor = new Visitor(Schema, this, Parameters);
			var result = visitor.ConvertToQueryExpression(expression);
			return new ExpressionResult(result,
				visitor.RequiredJoins.GroupBy(q => q).Select(q => q.First()).ToArray());
		}

		private class Visitor : ExpressionVisitor
		{
			public Schema.Schema Schema { get; }

			public List<Join> RequiredJoins { get; }
				= new List<Join>();

			private readonly ExpressionConverter _parent;
			private Dictionary<ParameterExpression, EntityModel> _expressionParameters;
			private Stack<QueryExpression> _queryExpressionStack = new Stack<QueryExpression>();

			public Visitor(Schema.Schema schema, ExpressionConverter parent,
				Dictionary<ParameterExpression, EntityModel> expressionParameters)
			{
				Schema = schema;
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

			private QueryExpression GetValueExpression(object value)
			{
				if (value is QueryExpression queryExpression)
					return queryExpression;

				if (value is IQueryBuilder queryBuilder)
					return queryBuilder.BuildQuery();

				return ORMQueryExpressions.Value(value);
			}

			protected override Expression VisitConstant(ConstantExpression node)
			{
				SetConversionResult(GetValueExpression(node.Value));
				return node;
			}

			protected override Expression VisitParameter(ParameterExpression node)
			{
				if (_expressionParameters.TryGetValue(node, out var entityModel))
				{
					SetConversionResult(
						QueryExpression.Table(entityModel.Table.TableName)
						);
				}
				return node;
			}

			protected override Expression VisitMethodCall(MethodCallExpression node)
			{
				var converter = Schema.GetMethodCallConverter(node.Method);
				if (converter == null)
					throw new Exception("Method call not supported.");
				var result = converter.Convert(node.Method, node, _parent);
				if (result != null)
				{
					if (result.RequiredJoins != null)
						RequiredJoins.AddRange(result.RequiredJoins);
					SetConversionResult(result.QueryExpression);
				}
				return node;
			}

			private EntityField ResolveEntityFieldFromPath(
				EntityModel entityModel, IEnumerable<string> path
				)
			{
				var fields = entityModel.Fields;
				var field = default(EntityField);
				var previousField = default(EntityField);
				foreach (var segment in path)
				{
					previousField = field;
					field = fields.FirstOrDefault(q => q.FieldName == segment);
					if (field == null)
						return null;

					fields = field.SubFields;
				}
				if (field.IsPrimaryKey && previousField != null)
					return previousField;
				return field;
			}

			protected override Expression VisitMember(MemberExpression node)
			{
				var (allExpressions, expressionPath) = FlattenExpressionTree(node);
				var entityModel = default(EntityModel);
				if (allExpressions[0] is ParameterExpression topParameterExpression)
				{
					_expressionParameters.TryGetValue(topParameterExpression, out entityModel);
				}
				if (entityModel != null)
				{
					//  visiting a member of expression parameter, ie. a field on the entity table
					var reflectionMemberInfo = node.Member;
					var sourceExpression = ConvertToQueryExpression(allExpressions[0]);
					var entityField = ResolveEntityFieldFromPath(
						entityModel, expressionPath.Skip(1)
						);
					if (entityField != null)
					{
						if (entityField.Source as Join != null)
						{
							sourceExpression = entityField.Source.AliasIdentifierExpression;
							if (!RequiredJoins.Contains(entityField.Source))
								RequiredJoins.Add(entityField.Source as Join);
						}
						if (entityField.Columns.Count > 1)
							//  todo: this is a problem with composite keys used in a relationship, I'm undecided what is useful to return here
							throw new InvalidOperationException("Field has multiple columns.");
						SetConversionResult(
							QueryExpression.Column(entityField.Columns[0].Name, sourceExpression)
						);
						return node;
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

				switch (node.NodeType)
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

	public class EntityExpressionConverter<T> : ExpressionConverter
		where T : class
	{
		public EntityModel<T> EntitySchema { get; }

		public EntityExpressionConverter(Schema.Schema schema)
			: base(schema)
		{
			EntitySchema = schema.GetAll<T>().First(); //  todo: replace with something more specific if there's multiple entity schemas defined for the type, or throw exception?
			if (EntitySchema == null)
				throw new Exception("Entity isn't configured in schema.");
		}

		public ExpressionResult Convert<TResult>(Expression<Func<T, TResult>> expression)
		{
			Parameters = new Dictionary<ParameterExpression, EntityModel>
			{
				{ expression.Parameters[0], EntitySchema }
			};
			return Convert((Expression)expression);
		}
	}
}
