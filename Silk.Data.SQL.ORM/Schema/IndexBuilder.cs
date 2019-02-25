using Silk.Data.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class IndexBuilder
	{
		public string IndexName { get; }
		public bool HasUniqueConstraint { get; set; }

		protected readonly List<IFieldPath<TypeModel, PropertyInfoField>> IndexFields
			= new List<IFieldPath<TypeModel, PropertyInfoField>>();

		public IndexBuilder(string indexName)
		{
			IndexName = indexName;
		}

		public abstract Index Build(IEnumerable<EntityField> modelFields);
	}

	public class IndexBuilder<T> : IndexBuilder
	{
		private readonly static TypeModel<T> _entityTypeModel = TypeModel.GetModelOf<T>();

		public IndexBuilder(string indexName) :
			base(indexName)
		{
		}

		public void AddFields(params Expression<Func<T, object>>[] indexFields)
		{
			foreach (var indexField in indexFields)
			{
				AddField(indexField);
			}
		}

		public override Index Build(IEnumerable<EntityField> modelFields)
		{
			return new Index(IndexName, HasUniqueConstraint,
				IndexFields.Select(q => ResolveField(q, modelFields))
					.Where(q => q != null).ToArray());
		}

		private EntityField ResolveField(IFieldPath<TypeModel, PropertyInfoField> fieldPath,
			IEnumerable<EntityField> modelFields)
		{
			var fields = modelFields;
			var foundField = default(EntityField);
			foreach (var field in fieldPath.Fields)
			{
				foundField = fields.FirstOrDefault(q => q.FieldName == field.FieldName);
				if (foundField == null)
					return null;

				fields = foundField.SubFields;
			}

			return foundField;
		}

		private void AddField(Expression indexField)
		{
			if (indexField is LambdaExpression lambdaExpression)
			{
				if (lambdaExpression.Body is UnaryExpression unaryExpression)
				{
					AddField(unaryExpression.Operand);
				}
				else if (lambdaExpression.Body is MemberExpression memberExpression)
				{
					AddField(memberExpression);
				}
			}
			else
			{
				var path = new List<string>();
				PopulatePath(indexField, path);

				var fieldPath = GetFieldPath(path);
				if (fieldPath == null)
					throw new ArgumentException("Field selector expression doesn't specify a valid member.", nameof(indexField));

				if (!IndexFields.Any(q => q.Fields.Select(q2 => q2.FieldName).SequenceEqual(path)))
					IndexFields.Add(fieldPath);
			}
		}

		private IFieldPath<TypeModel, PropertyInfoField> GetFieldPath(IEnumerable<string> path)
		{
			var fieldPath = new FieldPath<TypeModel, PropertyInfoField>(
				_entityTypeModel, null, new PropertyInfoField[0]
				);
			foreach (var segment in path)
			{
				var field = _entityTypeModel.GetPathFields(fieldPath)
					.FirstOrDefault(q => q.FieldName == segment);
				if (field == null)
					return null;

				fieldPath = fieldPath.Child(field);
			}

			return fieldPath;
		}

		private void PopulatePath(Expression expression, List<string> path)
		{
			if (expression is MemberExpression memberExpression)
			{
				var parentExpr = memberExpression.Expression;
				PopulatePath(parentExpr, path);

				path.Add(memberExpression.Member.Name);
			}
		}
	}
}
