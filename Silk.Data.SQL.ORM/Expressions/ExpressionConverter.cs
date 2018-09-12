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

		protected Dictionary<ParameterExpression, EntitySchema> Parameters { get; set; }
		protected Relationship Relationship { get; set; }

		public ExpressionConverter(Schema.Schema schema)
		{
			Schema = schema;
		}

		public ExpressionResult Convert(Expression expression)
		{
			var visitor = new Visitor(Schema, this, Parameters, Relationship);
			var result = visitor.ConvertToQueryExpression(expression);
			return new ExpressionResult(result,
				visitor.RequiredJoins.GroupBy(q => q).Select(q => q.First()).ToArray());
		}

		private class Visitor : ExpressionVisitor
		{
			public Schema.Schema Schema { get; }

			public List<EntityFieldJoin> RequiredJoins { get; }
				= new List<EntityFieldJoin>();

			private readonly ExpressionConverter _parent;
			private Dictionary<ParameterExpression, EntitySchema> _expressionParameters;
			private readonly Relationship _relationship;
			private Stack<QueryExpression> _queryExpressionStack = new Stack<QueryExpression>();

			public Visitor(Schema.Schema schema, ExpressionConverter parent,
				Dictionary<ParameterExpression, EntitySchema> expressionParameters,
				Relationship relationship)
			{
				Schema = schema;
				_parent = parent;
				_expressionParameters = expressionParameters;
				_relationship = relationship;
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
				if (_expressionParameters.TryGetValue(node, out var schema))
				{
					SetConversionResult(
						QueryExpression.Table(schema.EntityTable.TableName)
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

			private (ForeignKey, string) ResolveForeignKey(Column column, List<EntityFieldJoin> joinChain = null)
			{
				if (_relationship == null && joinChain == null)
					return (null, null);

				if (joinChain != null && joinChain.Count > 0)
				{
					var testJoin = joinChain.Last();
					var fk = testJoin.EntityField.ForeignKeys.FirstOrDefault(q => q.ForeignColumn == column);
					if (fk != null)
					{
						return (fk, testJoin.SourceName);
					}
				}

				if (_relationship != null)
				{
					var fk = _relationship.LeftRelationship.ForeignKeys
						.FirstOrDefault(q => q.ForeignColumn == column);
					if (fk != null)
						return (fk, _relationship.JunctionTable.TableName);

					fk = _relationship.RightRelationship.ForeignKeys
						.FirstOrDefault(q => q.ForeignColumn == column);
					return (fk, _relationship.JunctionTable.TableName);
				}

				return (null, null);
			}

			protected override Expression VisitMember(MemberExpression node)
			{
				var (allExpressions, expressionPath) = FlattenExpressionTree(node);
				var entitySchema = default(EntitySchema);
				if (allExpressions[0] is ParameterExpression topParameterExpression)
				{
					_expressionParameters.TryGetValue(topParameterExpression, out entitySchema);
				}
				if (entitySchema != null)
				{
					//  visiting a member of expression parameter, ie. a field on the entity table
					var reflectionMemberInfo = node.Member;
					var sourceExpression = ConvertToQueryExpression(allExpressions[0]);
					var entityField = entitySchema.EntityFields
						.FirstOrDefault(q => q.ModelPath.SequenceEqual(expressionPath.Skip(1))) as IEntityField;
					if (entityField != null && SqlTypeHelper.IsSqlPrimitiveType(entityField.DataType))
					{
						var (foreignKey, foreignKeyTable) = ResolveForeignKey(entityField.Columns[0]);
						if (foreignKey != null)
						{
							SetConversionResult(
								QueryExpression.Column(foreignKey.LocalColumn.ColumnName, QueryExpression.Table(foreignKeyTable))
								);
						}
						else
						{
							SetConversionResult(
								QueryExpression.Column(entityField.Columns[0].ColumnName, sourceExpression)
								);
							if (_relationship != null)
							{
								RequiredJoins.Add(
									entitySchema.EntityType == _relationship.LeftType ?
										_relationship.LeftJoin : _relationship.RightJoin
									);
							}
						}
						return node;
					}

					if (entityField == null)
					{
						//  entity field not found on model, search for the entity field through a JOIN tree
						var currentSchema = entitySchema;
						var joinChain = new List<EntityFieldJoin>();

						if (_relationship != null)
						{
							joinChain.Add(
								entitySchema.EntityType == _relationship.LeftType ?
									_relationship.LeftJoin : _relationship.RightJoin
								);
						}

						foreach (var pathSegment in expressionPath.Skip(1))
						{
							entityField = currentSchema.EntityFields.FirstOrDefault(q => q.FieldName == pathSegment);
							if (entityField == null)
								throw new Exception("Couldn't resolve entity field on related object.");

							if (entityField.KeyType == KeyType.ManyToOne)
							{
								var join = currentSchema.EntityJoins.FirstOrDefault(
									q => q.EntityField == entityField
									);
								if (join == null)
									throw new Exception("Couldn't resolve JOIN for related field.");

								if (_relationship != null && joinChain.Count == 1)
								{
									//  make a modified join that will work with the many-to-many join
									join = new EntityFieldJoin(
										join.TableName, join.TableAlias, joinChain[0].TableAlias, join.LeftColumns,
										join.RightColumns, join.EntityField, join.DependencyJoins
										);
								}

								joinChain.Add(join);
								currentSchema = Schema.GetEntitySchema(entityField.DataType);
							}
						}

						if (entityField != null && SqlTypeHelper.IsSqlPrimitiveType(entityField.DataType))
						{
							var (foreignKey, foreignKeyTable) = ResolveForeignKey(entityField.Columns[0],
								joinChain);

							if (foreignKey != null)
							{
								SetConversionResult(
									QueryExpression.Column(foreignKey.LocalColumn.ColumnName, new AliasIdentifierExpression(foreignKeyTable))
									);
								RequiredJoins.AddRange(joinChain.Take(joinChain.Count - 1));
							}
							else
							{
								sourceExpression = new AliasIdentifierExpression(
									joinChain.Last().TableAlias
									);
								SetConversionResult(
									QueryExpression.Column(entityField.Columns[0].ColumnName, sourceExpression)
									);
								RequiredJoins.AddRange(joinChain);
							}
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

	public class ExpressionConverter<T> : ExpressionConverter
		where T : class
	{
		public EntitySchema<T> EntitySchema { get; }

		public ExpressionConverter(Schema.Schema schema)
			: base(schema)
		{
			EntitySchema = schema.GetEntitySchema<T>();
			if (EntitySchema == null)
				throw new Exception("Entity isn't configured in schema.");
		}

		public ExpressionResult Convert<TResult>(Expression<Func<T, TResult>> expression)
		{
			Parameters = new Dictionary<ParameterExpression, EntitySchema>
			{
				{ expression.Parameters[0], EntitySchema }
			};
			return Convert((Expression)expression);
		}
	}

	public class ExpressionConverter<TLeft, TRight> : ExpressionConverter
		where TLeft : class
		where TRight : class
	{
		public EntitySchema<TLeft> LeftEntitySchema { get; }
		public EntitySchema<TRight> RightEntitySchema { get; }

		public ExpressionConverter(Schema.Schema schema, string relationshipName)
			: base(schema)
		{
			Relationship = schema.GetRelationship<TLeft, TRight>(relationshipName);
			if (Relationship == null)
				throw new Exception("Relationship isn't configured in schema.");

			LeftEntitySchema = schema.GetEntitySchema<TLeft>();
			RightEntitySchema = schema.GetEntitySchema<TRight>();
		}

		public ExpressionResult Convert<TResult>(Expression<Func<TLeft, TRight, TResult>> expression)
		{
			Parameters = new Dictionary<ParameterExpression, EntitySchema>
			{
				{ expression.Parameters[0], LeftEntitySchema },
				{ expression.Parameters[1], RightEntitySchema }
			};
			return Convert((Expression)expression);
		}
	}
}
