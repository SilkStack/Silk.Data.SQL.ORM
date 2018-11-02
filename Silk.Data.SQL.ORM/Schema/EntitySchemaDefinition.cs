﻿using Silk.Data.Modelling;
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
		/// <summary>
		/// Gets or sets the table name to store entity instances in.
		/// </summary>
		string TableName { get; set; }
	}

	/// <summary>
	/// Defines the schema components for entities of type T.
	/// </summary>
	public class EntitySchemaDefinition<T> : IEntitySchemaDefinition
		where T : class
	{
		private readonly TypeModel<T> _entityTypeModel = TypeModel.GetModelOf<T>();
		private readonly Dictionary<IPropertyField, EntityFieldDefinition> _entityFieldBuilders
			= new Dictionary<IPropertyField, EntityFieldDefinition>();
		private readonly Dictionary<string, SchemaIndexBuilder<T>> _indexBuilders
			= new Dictionary<string, SchemaIndexBuilder<T>>();

		public string TableName { get; set; }

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

		public EntityFieldDefinition<TProperty, T> For<TProperty>(PropertyField<TProperty> propertyField)
		{
			if (_entityFieldBuilders.TryGetValue(propertyField, out var builder))
				return builder as EntityFieldDefinition<TProperty, T>;

			builder = new EntityFieldDefinition<TProperty, T>(propertyField);
			_entityFieldBuilders.Add(propertyField, builder);
			return builder as EntityFieldDefinition<TProperty, T>;
		}

		public virtual EntityFieldDefinition<TProperty, T> For<TProperty>(Expression<Func<T, TProperty>> property)
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

		public EntityFieldDefinition<TProperty, T> For<TProperty>(Expression<Func<T, TProperty>> property,
			Action<EntityFieldDefinition<TProperty, T>> configureCallback)
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

		/// <summary>
		/// Builds the entity schema.
		/// </summary>
		/// <returns></returns>
		//public override PartialEntitySchema BuildPartialSchema(PartialEntitySchemaCollection partialEntities)
		//{
		//	return new PartialEntitySchema<T>(
		//		BuildEntityFields(_entityTypeModel, getPrimitiveFields: true, entityPrimitiveFields: partialEntities).ToArray(),
		//		TableName
		//		);
		//}

		//public override bool DefineNewSchemaFields(PartialEntitySchemaCollection partialEntities)
		//{
		//	var additionalFields = BuildEntityFields(_entityTypeModel, getPrimitiveFields: false, entityPrimitiveFields: partialEntities)
		//		.ToArray();
		//	if (additionalFields.Length > 0)
		//	{
		//		partialEntities[typeof(T)].EntityFields.AddRange(additionalFields);
		//		return true;
		//	}
		//	return false;
		//}

		//public override EntitySchema BuildSchema(PartialEntitySchemaCollection partialEntities)
		//{
		//	var fields = partialEntities[typeof(T)].EntityFields.OfType<IEntityFieldOfEntity<T>>().ToArray();
		//	var columns = fields.SelectMany(q => q.Columns).ToArray();
		//	var joins = BuildManyToOneJoins(_entityTypeModel, fields, partialEntities, TableName).ToArray();
		//	var projectionFields = BuildProjectionFields(_entityTypeModel, fields, partialEntities, joins).ToArray();
		//	var table = new Table(TableName, columns);
		//	var mapping = new Mapping(_entityTypeModel, null,
		//		BuildBindings(_entityTypeModel, fields, partialEntities, joins).ToArray()
		//		);
		//	return new EntitySchema<T>(
		//		table, fields, projectionFields, joins,
		//		_indexBuilders.Select(kvp => kvp.Value.Build(table, fields)).ToArray(),
		//		mapping
		//		);
		//}

		//private IEnumerable<EntityFieldJoin> BuildManyToOneJoins(
		//	TypeModel entityModel, IEnumerable<IEntityField> entityFields,
		//	PartialEntitySchemaCollection partialEntities, string currentSourceName, string[] propertyPath = null,
		//	EntityFieldJoin[] dependencyJoins = null)
		//{
		//	if (propertyPath == null)
		//		propertyPath = new string[0];
		//	if (dependencyJoins == null)
		//		dependencyJoins = new EntityFieldJoin[0];

		//	foreach (var modelField in entityModel.Fields)
		//	{
		//		if (SqlTypeHelper.IsSqlPrimitiveType(modelField.FieldType))
		//			continue;

		//		var subPropertyPath = propertyPath.Concat(new[] { modelField.FieldName }).ToArray();
		//		if (partialEntities.IsEntityTypeDefined(modelField.FieldType))
		//		{
		//			var relatedEntityType = partialEntities[modelField.FieldType];
		//			var joinAliasName = $"__joinAlias_{string.Join("_", subPropertyPath)}";
		//			var entityField = entityFields.First(q => q.ModelField == modelField);

		//			var primaryKeyFields = partialEntities.GetEntityPrimaryKeys(entityField.DataType);
		//			var foreignPrimaryKeyColumnNames = primaryKeyFields.Select(q => q.EntityField.Columns[0].ColumnName).ToArray();
		//			var localPrimaryKeyColumnNames = entityField.Columns.Select(q => q.ColumnName).ToArray();

		//			var newJoin = new EntityFieldJoin(
		//				relatedEntityType.TableName,
		//				joinAliasName,
		//				currentSourceName,
		//				localPrimaryKeyColumnNames,
		//				foreignPrimaryKeyColumnNames,
		//				entityField,
		//				dependencyJoins
		//				);
		//			yield return newJoin;

		//			foreach (var subJoin in BuildManyToOneJoins(
		//				modelField.FieldTypeModel,
		//				relatedEntityType.EntityFields.Select(q => q.EntityField),
		//				partialEntities,
		//				joinAliasName,
		//				subPropertyPath,
		//				dependencyJoins.Concat(new[] { newJoin }).ToArray()))
		//				yield return subJoin;
		//		}
		//		else
		//		{
		//			foreach (var subJoin in BuildManyToOneJoins(
		//				modelField.FieldTypeModel,
		//				entityFields,
		//				partialEntities,
		//				currentSourceName,
		//				subPropertyPath,
		//				dependencyJoins
		//				))
		//				yield return subJoin;
		//		}
		//	}
		//}

		//private IEnumerable<CoreBinding.Binding> BuildBindings(
		//	TypeModel entityModel, IEnumerable<IEntityField> entityFields,
		//	PartialEntitySchemaCollection partialEntities, EntityFieldJoin[] entityJoins,
		//	IEntityField joinEntityField = null, string[] propertyPath = null)
		//{
		//	if (propertyPath == null)
		//		propertyPath = new string[0];

		//	foreach (var modelField in entityModel.Fields)
		//	{
		//		var subPropertyPath = propertyPath.Concat(new[] { modelField.FieldName }).ToArray();

		//		if (SqlTypeHelper.IsSqlPrimitiveType(modelField.FieldType))
		//		{
		//			var entityField = entityFields.FirstOrDefault(q => q.ModelField == modelField);
		//			if (entityField == null) //  opted out of modelling this field
		//				continue;
		//			var entityJoin = entityJoins.FirstOrDefault(q => q.EntityField == joinEntityField);
		//			var sourceName = entityJoin?.TableAlias ?? TableName;
		//			var prefix = propertyPath.Length == 0 ? "" : $"{string.Join("_", propertyPath)}_";

		//			//yield return entityField.BuildProjectionField(sourceName,
		//			//	entityField.Columns[0].ColumnName,
		//			//	$"{prefix}{entityField.ModelField.FieldName}",
		//			//	subPropertyPath, entityJoin);
		//		}
		//		else if (partialEntities.IsEntityTypeDefined(modelField.FieldType))
		//		{
		//			var entityField = entityFields.FirstOrDefault(q => q.ModelField == modelField);
		//			if (entityField == null) //  opted out of modelling this field
		//				continue;
		//			var relatedEntityType = partialEntities[modelField.FieldType];

		//			var entityJoin = entityJoins.FirstOrDefault(q => q.EntityField == entityField);
		//			var prefix = propertyPath.Length == 0 ? "" : $"{string.Join("_", propertyPath)}_";

		//			//  todo: decide how to support the null check binding this projection provides
		//			//        on joins with composite keys
		//			foreach (var foreignKey in entityField.ForeignKeys)
		//			{
		//				//yield return foreignKey.BuildProjectionField(entityJoin.TableAlias,
		//				//	foreignKey.ForeignColumn.ColumnName,
		//				//	$"__NULL_CHECK_{prefix}{entityField.ModelField.FieldName}",
		//				//	subPropertyPath);
		//			}

		//			//foreach (var relatedEntityField in BuildProjectionFields(modelField.FieldTypeModel, relatedEntityType.EntityFields, partialEntities, entityJoins, entityField, subPropertyPath))
		//			//	yield return relatedEntityField;
		//		}
		//		else
		//		{
		//			var entityField = entityFields.FirstOrDefault(q => q.ModelField == modelField);
		//			if (entityField == null) //  opted out of modelling this field
		//				continue;
		//			var entityJoin = entityJoins.FirstOrDefault(q => q.EntityField == joinEntityField);
		//			var sourceName = entityJoin?.TableAlias ?? TableName;
		//			var prefix = propertyPath.Length == 0 ? "" : $"{string.Join("_", propertyPath)}_";

		//			//yield return entityField.BuildProjectionField(sourceName,
		//			//	entityField.Columns[0].ColumnName,
		//			//	$"__NULL_CHECK_{prefix}{entityField.ModelField.FieldName}",
		//			//	subPropertyPath, entityJoin);

		//			//foreach (var relatedEntityField in BuildProjectionFields(modelField.FieldTypeModel, entityFields, partialEntities, entityJoins, joinEntityField, subPropertyPath))
		//			//	yield return relatedEntityField;
		//		}
		//	}

		//	yield break;
		//}

		//private IEnumerable<ProjectionField> BuildProjectionFields(
		//	TypeModel entityModel, IEnumerable<IEntityField> entityFields,
		//	PartialEntitySchemaCollection partialEntities, EntityFieldJoin[] entityJoins,
		//	IEntityField joinEntityField = null, string[] propertyPath = null)
		//{
		//	if (propertyPath == null)
		//		propertyPath = new string[0];

		//	foreach (var modelField in entityModel.Fields)
		//	{
		//		var subPropertyPath = propertyPath.Concat(new[] { modelField.FieldName }).ToArray();

		//		if (SqlTypeHelper.IsSqlPrimitiveType(modelField.FieldType))
		//		{
		//			var entityField = entityFields.FirstOrDefault(q => q.ModelField == modelField);
		//			if (entityField == null) //  opted out of modelling this field
		//				continue;
		//			var entityJoin = entityJoins.FirstOrDefault(q => q.EntityField == joinEntityField);
		//			var sourceName = entityJoin?.TableAlias ?? TableName;
		//			var prefix = propertyPath.Length == 0 ? "" : $"{string.Join("_", propertyPath)}_";

		//			yield return entityField.BuildProjectionField(sourceName,
		//				entityField.Columns[0].ColumnName,
		//				$"{prefix}{entityField.ModelField.FieldName}",
		//				subPropertyPath, entityJoin);
		//		}
		//		else if (partialEntities.IsEntityTypeDefined(modelField.FieldType))
		//		{
		//			var entityField = entityFields.FirstOrDefault(q => q.ModelField == modelField);
		//			if (entityField == null) //  opted out of modelling this field
		//				continue;
		//			var relatedEntityType = partialEntities[modelField.FieldType];

		//			var entityJoin = entityJoins.FirstOrDefault(q => q.EntityField == entityField);
		//			var prefix = propertyPath.Length == 0 ? "" : $"{string.Join("_", propertyPath)}_";

		//			//  todo: decide how to support the null check binding this projection provides
		//			//        on joins with composite keys
		//			foreach (var foreignKey in entityField.ForeignKeys)
		//			{
		//				yield return foreignKey.BuildProjectionField(entityJoin.TableAlias,
		//					foreignKey.ForeignColumn.ColumnName,
		//					$"__NULL_CHECK_{prefix}{entityField.ModelField.FieldName}",
		//					subPropertyPath);
		//			}

		//			foreach (var relatedEntityField in BuildProjectionFields(modelField.FieldTypeModel,
		//				relatedEntityType.EntityFields.Select(q => q.EntityField),
		//				partialEntities, entityJoins, entityField, subPropertyPath))
		//				yield return relatedEntityField;
		//		}
		//		else
		//		{
		//			var entityField = entityFields.FirstOrDefault(q => q.ModelField == modelField);
		//			if (entityField == null) //  opted out of modelling this field
		//				continue;
		//			var entityJoin = entityJoins.FirstOrDefault(q => q.EntityField == joinEntityField);
		//			var sourceName = entityJoin?.TableAlias ?? TableName;
		//			var prefix = propertyPath.Length == 0 ? "" : $"{string.Join("_", propertyPath)}_";

		//			yield return entityField.BuildProjectionField(sourceName,
		//				entityField.Columns[0].ColumnName,
		//				$"__NULL_CHECK_{prefix}{entityField.ModelField.FieldName}",
		//				subPropertyPath, entityJoin);

		//			foreach (var relatedEntityField in BuildProjectionFields(modelField.FieldTypeModel, entityFields, partialEntities, entityJoins, joinEntityField, subPropertyPath))
		//				yield return relatedEntityField;
		//		}
		//	}
		//}

		//private IEnumerable<BuiltEntityField> BuildEntityFields(
		//	TypeModel entityTypeModel, bool getPrimitiveFields,
		//	PartialEntitySchemaCollection entityPrimitiveFields = null,
		//	string[] propertyPath = null)
		//{
		//	string propertyNamePrefix;
		//	if (propertyPath == null)
		//	{
		//		propertyPath = new string[0];
		//		propertyNamePrefix = "";
		//	}
		//	else
		//	{
		//		propertyNamePrefix = $"{string.Join("_", propertyPath)}_";
		//	}

		//	foreach (var modelField in entityTypeModel.Fields)
		//	{
		//		var subPropertyPath = propertyPath.Concat(new[] { modelField.FieldName }).ToArray();
		//		var isPrimitiveType = SqlTypeHelper.IsSqlPrimitiveType(modelField.FieldType);

		//		if (isPrimitiveType)
		//		{
		//			if (!getPrimitiveFields)
		//				continue;

		//			var builder = GetFieldBuilder(modelField);
		//			if (builder == null || !builder.IsModelled)
		//				continue;

		//			var entityField = builder.Build(propertyNamePrefix, subPropertyPath);
		//			if (entityField == null)
		//				continue;

		//			yield return entityField;
		//		}
		//		else if (entityPrimitiveFields.IsEntityTypeDefined(modelField.FieldType))
		//		{
		//			if (getPrimitiveFields)
		//				continue;

		//			//  many to one relationship
		//			var primaryKeyFields = entityPrimitiveFields?.GetEntityPrimaryKeys(modelField.FieldType)?.ToArray();
		//			if (primaryKeyFields == null || primaryKeyFields.Length == 0)
		//				throw new Exception("Related entity types must have a primary key.");

		//			var relatedEntityField = entityPrimitiveFields[typeof(T)]
		//				.EntityFields.FirstOrDefault(q => q.EntityField.ModelField == modelField);
		//			if (relatedEntityField == null)
		//			{
		//				relatedEntityField = entityPrimitiveFields[modelField.FieldType]
		//					.CreateRelatedEntityField<T>(modelField.FieldName, modelField.FieldType,
		//					modelField, entityPrimitiveFields, modelField.FieldName, subPropertyPath);
		//				yield return relatedEntityField;
		//			}
		//		}
		//		else
		//		{
		//			//  embedded POCO
		//			var embeddedEntityField = entityPrimitiveFields[typeof(T)]
		//				?.EntityFields.FirstOrDefault(q => q.EntityField.ModelField == modelField);
		//			if (embeddedEntityField == null)
		//			{
		//				var builder = GetFieldBuilder(modelField);
		//				if (builder == null || !builder.IsModelled)
		//					continue;

		//				embeddedEntityField = builder.Build(propertyNamePrefix, subPropertyPath);
		//				if (embeddedEntityField == null)
		//					continue;

		//				yield return embeddedEntityField;
		//			}
		//			//  go deeper!
		//			foreach (var field in BuildEntityFields(modelField.FieldTypeModel, getPrimitiveFields, entityPrimitiveFields, subPropertyPath))
		//				yield return field;
		//		}
		//	}
		//}
	}
}