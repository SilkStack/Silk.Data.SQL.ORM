﻿using Silk.Data.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Builds the schema components for a specific entity type.
	/// </summary>
	public abstract class EntitySchemaBuilder
	{
		/// <summary>
		/// Builds and returns an enumeration of primitive fields to be stored with no embedded or related objects taken into consideration.
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<EntityField> BuildPrimitiveFields();

		/// <summary>
		/// Builds the completed entity schema.
		/// </summary>
		/// <returns></returns>
		public abstract EntitySchema BuildSchema(EntityPrimitiveFieldCollection entityPrimitiveFields);
	}

	/// <summary>
	/// Builds the schema components for entities of type T.
	/// </summary>
	public class EntitySchemaBuilder<T> : EntitySchemaBuilder
	{
		private readonly TypeModel<T> _entityTypeModel = TypeModel.GetModelOf<T>();
		private readonly Dictionary<IPropertyField, EntityFieldBuilder> _entityFieldBuilders
			= new Dictionary<IPropertyField, EntityFieldBuilder>();

		public virtual EntityFieldBuilder<TProperty> For<TProperty>(Expression<Func<T, TProperty>> property)
		{
			if (property.Body is MemberExpression memberExpression)
			{
				var path = new List<string>();
				PopulatePath(property.Body, path);

				var field = GetField(path);
				if (field == null)
					throw new ArgumentException("Field selector expression doesn't specify a valid member.", nameof(property));

				if (_entityFieldBuilders.TryGetValue(field, out var fieldBuilder))
					return fieldBuilder as EntityFieldBuilder<TProperty>;

				fieldBuilder = new EntityFieldBuilder<TProperty>(field);
				_entityFieldBuilders.Add(field, fieldBuilder);
				return fieldBuilder as EntityFieldBuilder<TProperty>;
			}
			throw new ArgumentException("Field selector must be a MemberExpression.", nameof(property));
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

		/// <summary>
		/// Builds the entity schema.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<EntityField> BuildPrimitiveFields()
		{
			return BuildEntityFields(getPrimitiveFields: true);
		}

		public override EntitySchema BuildSchema(EntityPrimitiveFieldCollection entityPrimitiveFields)
		{
			return new EntitySchema<T>(new Table(),
				entityPrimitiveFields[typeof(T)]
					.Concat(BuildEntityFields(getPrimitiveFields: false))
					.ToArray()
				);
		}

		private IEnumerable<EntityField> BuildEntityFields(bool getPrimitiveFields)
		{
			foreach (var modelField in _entityTypeModel.Fields)
			{
				var isPrimitiveType = SqlTypeHelper.IsSqlPrimitiveType(modelField.FieldType);

				if (getPrimitiveFields != isPrimitiveType)
					continue;

				var builder = GetFieldBuilder(modelField);
				if (builder == null)
					continue;

				var entityField = builder.Build();
				if (entityField == null)
					continue;

				yield return entityField;
			}
		}

		private EntityFieldBuilder GetFieldBuilder(IPropertyField propertyField)
		{
			if (_entityFieldBuilders.TryGetValue(propertyField, out var builder))
				return builder;
			builder = CreateFieldBuilder(propertyField);
			_entityFieldBuilders.Add(propertyField, builder);
			return builder;
		}

		private EntityFieldBuilder CreateFieldBuilder(IPropertyField propertyField)
		{
			//  reflection justification:
			//    building a schema is a rare occurance, using reflection here should make the use of generic types
			//    possible through the entire codebase while only executing slow reflection code this once
			return Activator.CreateInstance(
				typeof(EntityFieldBuilder<>).MakeGenericType(propertyField.FieldType),
				new object[] { propertyField }
				) as EntityFieldBuilder;
		}
	}
}
