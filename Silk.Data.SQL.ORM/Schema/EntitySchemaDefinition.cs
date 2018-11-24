using Silk.Data.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Defines the schema components for a specific entity type.
	/// </summary>
	public interface IEntitySchemaDefinition
	{
		Guid DefinitionId { get; }

		Type EntityType { get; }

		/// <summary>
		/// Gets or sets the table name to store entity instances in.
		/// </summary>
		string TableName { get; set; }

		IEnumerable<ISchemaIndexBuilder> GetIndexBuilders();
	}

	/// <summary>
	/// Defines the schema components for entities of type T.
	/// </summary>
	public class EntitySchemaDefinition<T> : IEntitySchemaDefinition
		where T : class
	{
		private readonly TypeModel<T> _entityTypeModel = TypeModel.GetModelOf<T>();
		private readonly Dictionary<IPropertyField, SchemaFieldDefinition> _entityFieldBuilders
			= new Dictionary<IPropertyField, SchemaFieldDefinition>();
		private readonly Dictionary<string, SchemaIndexBuilder<T>> _indexBuilders
			= new Dictionary<string, SchemaIndexBuilder<T>>();

		public Type EntityType => typeof(T);

		public string TableName { get; set; }

		public Guid DefinitionId { get; } = Guid.NewGuid();

		public EntitySchemaDefinition()
		{
			TableName = typeof(T).Name;
		}

		public SchemaIndexBuilder<T> Index(string indexName, params Expression<Func<T, object>>[] indexFields)
			=> Index(indexName, null, indexFields);

		public SchemaIndexBuilder<T> Index(string indexName, bool? uniqueConstraint, params Expression<Func<T, object>>[] indexFields)
		{
			if (!_indexBuilders.TryGetValue(indexName, out var indexBuilder))
			{
				indexBuilder = new SchemaIndexBuilder<T>(indexName);
				_indexBuilders.Add(indexName, indexBuilder);
			}
			if (uniqueConstraint != null)
				indexBuilder.HasUniqueConstraint = uniqueConstraint.Value;
			indexBuilder.AddFields(indexFields);
			return indexBuilder;
		}

		public bool IsModelled(IPropertyField propertyField)
		{
			if (_entityFieldBuilders.TryGetValue(propertyField, out var builder))
				return builder.IsModelled;
			return true;
		}

		public SchemaFieldDefinition<TProperty, T> For<TProperty>(PropertyField<TProperty> propertyField)
		{
			if (_entityFieldBuilders.TryGetValue(propertyField, out var builder))
				return builder as SchemaFieldDefinition<TProperty, T>;

			builder = new SchemaFieldDefinition<TProperty, T>(propertyField);
			_entityFieldBuilders.Add(propertyField, builder);
			return builder as SchemaFieldDefinition<TProperty, T>;
		}

		public virtual SchemaFieldDefinition<TProperty, T> For<TProperty>(Expression<Func<T, TProperty>> property)
		{
			if (property.Body is MemberExpression memberExpression)
			{
				var path = new List<string>();
				PopulatePath(property.Body, path);

				var field = GetField<TProperty>(path);
				if (field == null)
					throw new ArgumentException("Field selector expression doesn't specify a valid member.", nameof(property));

				return For(field);
			}
			throw new ArgumentException("Field selector must be a MemberExpression.", nameof(property));
		}

		public SchemaFieldDefinition<TProperty, T> For<TProperty>(Expression<Func<T, TProperty>> property,
			Action<SchemaFieldDefinition<TProperty, T>> configureCallback)
		{
			var builder = For(property);
			configureCallback?.Invoke(builder);
			return builder;
		}

		private PropertyField<TProperty> GetField<TProperty>(IEnumerable<string> path)
		{
			var fields = _entityTypeModel.Fields;
			var field = default(IPropertyField);
			foreach (var segment in path)
			{
				field = fields.FirstOrDefault(q => q.FieldName == segment);
				fields = field.FieldTypeModel?.Fields;
			}
			return field as PropertyField<TProperty>;
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

		public IEnumerable<ISchemaIndexBuilder> GetIndexBuilders()
			=> _indexBuilders.Values;
	}
}
