using Silk.Data.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
		public abstract PartialEntitySchema BuildPartialSchema();

		/// <summary>
		/// Define new schema fields based on fields defined by other entities.
		/// </summary>
		/// <param name="partialEntities"></param>
		/// <returns>True if any new fields were defined.</returns>
		public abstract bool DefineNewSchemaFields(PartialEntitySchemaCollection partialEntities);

		/// <summary>
		/// Builds the completed entity schema.
		/// </summary>
		/// <returns></returns>
		public abstract EntitySchema BuildSchema(PartialEntitySchemaCollection partialEntities);

		/// <summary>
		/// Gets or sets the table name to store entity instances in.
		/// </summary>
		public string TableName { get; set; }
	}

	/// <summary>
	/// Builds the schema components for entities of type T.
	/// </summary>
	public class EntitySchemaBuilder<T> : EntitySchemaBuilder
	{
		private readonly TypeModel<T> _entityTypeModel = TypeModel.GetModelOf<T>();
		private readonly Dictionary<IPropertyField, EntityFieldBuilder> _entityFieldBuilders
			= new Dictionary<IPropertyField, EntityFieldBuilder>();

		public EntitySchemaBuilder()
		{
			TableName = typeof(T).Name;
		}

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
		public override PartialEntitySchema BuildPartialSchema()
		{
			return new PartialEntitySchema<T>(
				BuildEntityFields(getPrimitiveFields: true).ToArray(),
				TableName
				);
		}

		public override bool DefineNewSchemaFields(PartialEntitySchemaCollection partialEntities)
		{
			var additionalFields = BuildEntityFields(getPrimitiveFields: false, entityPrimitiveFields: partialEntities)
				.ToArray();
			if (additionalFields.Length > 0)
			{
				partialEntities[typeof(T)].EntityFields.AddRange(additionalFields);
				return true;
			}
			return false;
		}

		public override EntitySchema BuildSchema(PartialEntitySchemaCollection partialEntities)
		{
			var fields = partialEntities[typeof(T)].EntityFields.ToArray();
			var columns = fields.SelectMany(q => q.Columns).ToArray();
			var joins = BuildManyToOneJoins(fields, partialEntities, TableName).ToArray();
			var projectionFields = BuildProjectionFields(fields, partialEntities, joins).ToArray();
			return new EntitySchema<T>(
				new Table(TableName, columns), fields, projectionFields, joins
				);
		}

		private IEnumerable<EntityFieldJoin> BuildManyToOneJoins(IEnumerable<EntityField> entityFields,
			PartialEntitySchemaCollection partialEntities, string currentSourceName, string[] propertyPath = null,
			EntityFieldJoin[] dependencyJoins = null)
		{
			if (propertyPath == null)
				propertyPath = new string[0];
			if (dependencyJoins == null)
				dependencyJoins = new EntityFieldJoin[0];

			foreach (var entityField in entityFields)
			{
				if (SqlTypeHelper.IsSqlPrimitiveType(entityField.DataType))
					continue;

				var subPropertyPath = propertyPath.Concat(new[] { entityField.ModelField.FieldName }).ToArray();
				if (partialEntities.IsEntityTypeDefined(entityField.DataType))
				{
					var relatedEntityType = partialEntities[entityField.DataType];
					var primaryKeyFields = partialEntities.GetEntityPrimaryKeys(entityField.DataType);
					var foreignPrimaryKeyColumnNames = primaryKeyFields.Select(q => q.Columns[0].ColumnName).ToArray();
					var localPrimaryKeyColumnNames = entityField.Columns.Select(
						q => q.ColumnName
						).ToArray();
					var joinAliasName = $"__joinAlias_{string.Join("_", subPropertyPath)}";
					var newJoin = new EntityFieldJoin(
						relatedEntityType.TableName,
						joinAliasName,
						currentSourceName,
						localPrimaryKeyColumnNames,
						foreignPrimaryKeyColumnNames,
						entityField,
						dependencyJoins
						);
					yield return newJoin;

					foreach (var subJoin in BuildManyToOneJoins(
						relatedEntityType.EntityFields, partialEntities, joinAliasName,
						subPropertyPath, dependencyJoins.Concat(new[] { newJoin }).ToArray()
						))
						yield return subJoin;
				}
				else
				{

				}
			}
		}

		private IEnumerable<ProjectionField> BuildProjectionFields(IEnumerable<EntityField> entityFields,
			PartialEntitySchemaCollection partialEntities, EntityFieldJoin[] entityJoins, EntityField parentEntityField = null, string[] propertyPath = null)
		{
			if (propertyPath == null)
				propertyPath = new string[0];

			foreach (var entityField in entityFields)
			{
				var subPropertyPath = propertyPath.Concat(new[] { entityField.ModelField.FieldName }).ToArray();
				if (SqlTypeHelper.IsSqlPrimitiveType(entityField.DataType))
				{
					var entityJoin = entityJoins.FirstOrDefault(q => q.EntityField == parentEntityField);
					var sourceName = entityJoin?.TableAlias ?? TableName;
					var prefix = propertyPath.Length == 0 ? "" : $"{string.Join("_", propertyPath)}_";
					yield return new ProjectionField(sourceName,
						entityField.Columns[0].ColumnName,
						$"{prefix}{entityField.ModelField.FieldName}",
						subPropertyPath);
				}
				else if (partialEntities.IsEntityTypeDefined(entityField.DataType))
				{
					var relatedEntityType = partialEntities[entityField.DataType];
					foreach (var relatedEntityField in BuildProjectionFields(relatedEntityType.EntityFields, partialEntities, entityJoins, entityField, subPropertyPath))
						yield return relatedEntityField;
				}
				else
				{

				}
			}
		}

		private IEnumerable<EntityField> BuildEntityFields(bool getPrimitiveFields,
			PartialEntitySchemaCollection entityPrimitiveFields = null)
		{
			foreach (var modelField in _entityTypeModel.Fields)
			{
				var isPrimitiveType = SqlTypeHelper.IsSqlPrimitiveType(modelField.FieldType);

				if (getPrimitiveFields != isPrimitiveType)
					continue;

				if (isPrimitiveType)
				{
					var builder = GetFieldBuilder(modelField);
					if (builder == null)
						continue;

					var entityField = builder.Build();
					if (entityField == null)
						continue;

					yield return entityField;
				}
				else if (entityPrimitiveFields.IsEntityTypeDefined(modelField.FieldType))
				{
					//  many to one relationship
					var primaryKeyFields = entityPrimitiveFields?.GetEntityPrimaryKeys(modelField.FieldType)?.ToArray();
					if (primaryKeyFields == null || primaryKeyFields.Length == 0)
						throw new Exception("Related entity types must have a primary key.");

					var relatedEntityField = entityPrimitiveFields[typeof(T)]
						.EntityFields.FirstOrDefault(q => q.ModelField == modelField);
					if (relatedEntityField == null)
					{
						relatedEntityField = entityPrimitiveFields[modelField.FieldType]
							.CreateEntityField(modelField, entityPrimitiveFields, modelField.FieldName);
						yield return relatedEntityField;
					}
				}
				else
				{
					//  embedded POCO
				}
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
