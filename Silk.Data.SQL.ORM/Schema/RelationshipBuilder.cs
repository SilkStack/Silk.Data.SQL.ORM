using Silk.Data.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class RelationshipBuilder
	{
		public Type Left { get; }
		public Type Right { get; }
		public string Name { get; set; }

		public RelationshipBuilder(Type left, Type right)
		{
			Left = left;
			Right = right;
		}

		//public abstract Relationship Build(PartialEntitySchemaCollection partialEntities);
	}

	public class RelationshipBuilder<TLeft, TRight> : RelationshipBuilder
		where TLeft : class
		where TRight : class
	{
		private readonly TypeModel<TLeft> _leftTypeModel = TypeModel.GetModelOf<TLeft>();
		private readonly TypeModel<TRight> _rightTypeModel = TypeModel.GetModelOf<TRight>();
		private readonly Dictionary<PropertySource, Dictionary<IPropertyField, RelationshipFieldBuilder>>
			_relationshipFieldBuilders = new Dictionary<PropertySource, Dictionary<IPropertyField, RelationshipFieldBuilder>>
			{
				{ PropertySource.Left, new Dictionary<IPropertyField, RelationshipFieldBuilder>() },
				{ PropertySource.Right, new Dictionary<IPropertyField, RelationshipFieldBuilder>() }
			};

		public RelationshipBuilder() : base(typeof(TLeft), typeof(TRight)) { }

		private void PopulatePath(Expression expression, List<string> path)
		{
			if (expression is MemberExpression memberExpression)
			{
				var parentExpr = memberExpression.Expression;
				PopulatePath(parentExpr, path);

				path.Add(memberExpression.Member.Name);
			}
		}

		private IPropertyField GetField(IEnumerable<string> path, IPropertyField[] fields)
		{
			var field = default(IPropertyField);
			foreach (var segment in path)
			{
				field = fields.FirstOrDefault(q => q.FieldName == segment);
				fields = field.FieldTypeModel?.Fields;
			}
			return field;
		}

		public RelationshipFieldBuilder For<TProperty>(Expression<Func<TLeft, TRight, TProperty>> property)
		{
			if (property.Body is MemberExpression memberExpression)
			{
				var sourceParameterExpression = memberExpression.Expression as ParameterExpression;
				var source = PropertySource.Unknown;
				if (ReferenceEquals(property.Parameters[0], sourceParameterExpression))
					source = PropertySource.Left;
				else if (ReferenceEquals(property.Parameters[1], sourceParameterExpression))
					source = PropertySource.Right;

				if (source == PropertySource.Unknown)
					throw new Exception("Couldn't resolve property.");

				var path = new List<string>();
				PopulatePath(property.Body, path);

				var field = GetField(path, source == PropertySource.Left ? _leftTypeModel.Fields : _rightTypeModel.Fields);
				if (field == null)
					throw new ArgumentException("Field selector expression doesn't specify a valid member.", nameof(property));

				if (_relationshipFieldBuilders[source].TryGetValue(field, out var fieldBuilder))
					return fieldBuilder;

				fieldBuilder = new RelationshipFieldBuilder(field);
				_relationshipFieldBuilders[source].Add(field, fieldBuilder);
				return fieldBuilder;
			}
			throw new ArgumentException("Field selector must be a MemberExpression.", nameof(property));
		}

		//public override Relationship Build(PartialEntitySchemaCollection partialEntities)
		//{
		//	if (!partialEntities.IsEntityTypeDefined(typeof(TLeft)) ||
		//		!partialEntities.IsEntityTypeDefined(typeof(TRight)))
		//		throw new Exception("Related entity types not registered in schema.");

		//	var leftPartialSchema = partialEntities[typeof(TLeft)];
		//	var rightPartialSchema = partialEntities[typeof(TRight)];

		//	var leftRelationship = leftPartialSchema.CreateRelatedEntityField<TLeft>(
		//		"Left", typeof(TLeft),
		//		null, partialEntities, leftPartialSchema.TableName, new[] { "." },
		//		_relationshipFieldBuilders[PropertySource.Left].ToDictionary(
		//			q => q.Key,
		//			q => q.Value.ColumnName
		//			)
		//		);
		//	var rightRelationship = rightPartialSchema.CreateRelatedEntityField<TRight>(
		//		"Right", typeof(TRight),
		//		null, partialEntities, rightPartialSchema.TableName, new[] { "." },
		//		_relationshipFieldBuilders[PropertySource.Right].ToDictionary(
		//			q => q.Key,
		//			q => q.Value.ColumnName
		//			)
		//		);

		//	var table = new Table(Name, leftRelationship.Columns.Concat(rightRelationship.Columns).ToArray());

		//	var leftJoin = new EntityFieldJoin(leftPartialSchema.TableName, $"{Name}_{leftPartialSchema.TableName}",
		//		Name,
		//		leftRelationship.Columns.Select(q => q.ColumnName).ToArray(),
		//		partialEntities.GetEntityPrimaryKeys<TLeft>().SelectMany(q => q.EntityField.Columns).Select(q => q.ColumnName).ToArray(),
		//		leftRelationship, new EntityFieldJoin[0]);
		//	var rightJoin = new EntityFieldJoin(rightPartialSchema.TableName, $"{Name}_{rightPartialSchema.TableName}",
		//		Name,
		//		rightRelationship.Columns.Select(q => q.ColumnName).ToArray(),
		//		partialEntities.GetEntityPrimaryKeys<TRight>().SelectMany(q => q.EntityField.Columns).Select(q => q.ColumnName).ToArray(),
		//		rightRelationship, new EntityFieldJoin[0]);

		//	if (leftJoin.LeftColumns.Length == 0 || leftJoin.RightColumns.Length == 0 ||
		//		rightJoin.LeftColumns.Length == 0 || rightJoin.RightColumns.Length == 0)
		//	{
		//		throw new Exception("Relationship entities must have primary keys declared.");
		//	}

		//	return new Relationship<TLeft, TRight>(Name, table, leftRelationship, rightRelationship, leftJoin, rightJoin);
		//}

		private enum PropertySource
		{
			Unknown,
			Left,
			Right
		}
	}
}
