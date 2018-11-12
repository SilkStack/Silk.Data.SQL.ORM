using Silk.Data.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Schema
{
	public class SchemaIndexBuilder<T>
		where T : class
	{
		private readonly TypeModel<T> _entityTypeModel = TypeModel.GetModelOf<T>();

		private readonly List<IPropertyField> _indexFields = new List<IPropertyField>();

		public string IndexName { get; }
		public bool HasUniqueConstraint { get; set; }

		public SchemaIndexBuilder(string indexName)
		{
			IndexName = indexName;
		}

		public SchemaIndex Build(Table table, ISchemaField[] entityFields)
		{
			throw new NotImplementedException();
			//return new SchemaIndex(IndexName, HasUniqueConstraint, 
			//	entityFields.Where(q => _indexFields.Any(q2 => q.FieldName == q2.FieldName)).ToArray(),
			//	table);
		}

		public void AddFields(params Expression<Func<T, object>>[] indexFields)
		{
			foreach (var indexField in indexFields)
			{
				AddField(indexField);
			}
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

				var field = GetField(path);
				if (field == null)
					throw new ArgumentException("Field selector expression doesn't specify a valid member.", nameof(indexField));

				if (!_indexFields.Contains(field))
					_indexFields.Add(field);
			}
		}

		private IPropertyField GetField(IEnumerable<string> path)
		{
			var fields = _entityTypeModel.Fields;
			var field = default(IPropertyField);
			foreach (var segment in path)
			{
				field = fields.FirstOrDefault(q => q.FieldName == segment);
				fields = field.FieldTypeModel?.Fields;
			}
			return field;
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
