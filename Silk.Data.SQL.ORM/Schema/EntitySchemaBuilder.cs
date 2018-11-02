﻿using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Builds the schema components for a specific entity type.
	/// </summary>
	public interface IEntitySchemaBuilder
	{
		/// <summary>
		/// Creates the IEntitySchemaAssemblage that will be used to construct the EntitySchema populated with any declared fields that are SQL primitives and don't require any joins.
		/// </summary>
		/// <returns></returns>
		IEntitySchemaAssemblage CreateAssemblage();

		/// <summary>
		/// Define new schema fields based on fields defined by other entities.
		/// </summary>
		/// <param name="partialEntities"></param>
		/// <returns>True if any new fields were defined.</returns>
		bool DefineNewSchemaFields(PartialEntitySchemaCollection partialEntities);

		/// <summary>
		/// Builds the completed entity schema.
		/// </summary>
		/// <returns></returns>
		EntitySchema BuildSchema();
	}

	/// <summary>
	/// Builds the schema components for entities of type T.
	/// </summary>
	public class EntitySchemaBuilder<T> : IEntitySchemaBuilder
		where T : class
	{
		private readonly TypeModel<T> _entityTypeModel = TypeModel.GetModelOf<T>();
		private readonly EntitySchemaDefinition<T> _entitySchemaDefinition;
		private readonly IReadOnlyCollection<IEntitySchemaAssemblage> _entitySchemaAssemblages;
		private EntitySchemaAssemblage<T> _entitySchemaAssemblage;

		public EntitySchemaBuilder(
			EntitySchemaDefinition<T> entitySchemaDefinition,
			IReadOnlyCollection<IEntitySchemaAssemblage> entitySchemaAssemblages
			)
		{
			_entitySchemaDefinition = entitySchemaDefinition;
			_entitySchemaAssemblages = entitySchemaAssemblages;
		}

		public IEntitySchemaAssemblage CreateAssemblage()
		{
			_entitySchemaAssemblage = new EntitySchemaAssemblage<T>(
				!string.IsNullOrWhiteSpace(_entitySchemaDefinition.TableName) ?
					_entitySchemaDefinition.TableName :
					typeof(T).Name,
				_entitySchemaDefinition, this
				);
			EntityTypeVisitor.Visit(_entityTypeModel, VisitCallback);
			return _entitySchemaAssemblage;

			void VisitCallback(IPropertyField propertyField, Span<string> path)
			{
				if (path.Length != 1 || !SqlTypeHelper.IsSqlPrimitiveType(propertyField.FieldType))
					return;

				FindOrCreateField(propertyField, path);
			}
		}

		public EntitySchema BuildSchema()
		{
			var table = new Table(_entitySchemaAssemblage.TableName,
				_entitySchemaAssemblage.Columns.ToArray());
			var entityFields = GetEntityFields();
			var projectionFields = GetProjectionFields();
			var joins = new EntityFieldJoin[0];
			var indexes = new SchemaIndex[0];
			var mapping = default(Mapping);
			var entitySchema = new EntitySchema<T>(
				table, entityFields, projectionFields,
				joins, indexes, mapping
				);
			return entitySchema;
		}

		public bool DefineNewSchemaFields(PartialEntitySchemaCollection partialEntities)
		{
			throw new NotImplementedException();
		}

		private IEntityFieldOfEntity<T>[] GetEntityFields()
		{
			var entityFields = new List<IEntityFieldOfEntity<T>>();
			EntityTypeVisitor.Visit(_entityTypeModel, Callback);
			return entityFields.ToArray();

			void Callback(IPropertyField propertyField, Span<string> path)
			{
				if (SqlTypeHelper.IsSqlPrimitiveType(propertyField.FieldType))
				{
					var fieldAssemblage = FindOrCreateField(propertyField, path);
					if (fieldAssemblage.Builder.Build() is IEntityFieldOfEntity<T> entityField)
						entityFields.Add(entityField);
					return;
				}

				var fieldTypeAssemblage = _entitySchemaAssemblages.FirstOrDefault(
					q => q.EntityType == propertyField.FieldType
					);
				if (fieldTypeAssemblage == null)
				{
					entityFields.Add(new EmbeddedPocoField<T>(propertyField, path.ToArray()));
				}
				else
				{

				}
			}
		}

		private ProjectionField[] GetProjectionFields()
		{
			var projectionFields = new List<ProjectionField>();
			EntityTypeVisitor.Visit(_entityTypeModel, Callback);
			return projectionFields.ToArray();

			void Callback(IPropertyField propertyField, Span<string> path)
			{
				if (SqlTypeHelper.IsSqlPrimitiveType(propertyField.FieldType))
				{
					var fieldAssemblage = FindOrCreateField(propertyField, path);
					var sourceName = _entitySchemaAssemblage.TableName;
					var columnName = fieldAssemblage.FieldDefinition.ColumnName;
					var aliasName = string.Join("_", path.ToArray());
					var projectionField = fieldAssemblage.Builder.BuildProjectionField(
						sourceName, columnName, aliasName, fieldAssemblage.ModelPath, null
						);

					projectionFields.Add(projectionField);
					return;
				}

				var fieldTypeAssemblage = _entitySchemaAssemblages.FirstOrDefault(
					q => q.EntityType == propertyField.FieldType
					);
				if (fieldTypeAssemblage == null)
				{
					var sourceName = _entitySchemaAssemblage.TableName;
					var columnName = string.Join("_", path.ToArray());
					var aliasName = $"__NULL_CHECK_{string.Join("_", path.ToArray())}";
					projectionFields.Add(new EmbeddedPocoNullCheckProjection(
						sourceName, columnName, aliasName, path.ToArray(), null
						));
				}
				else
				{

				}
			}
		}

		private IEntityFieldAssemblage FindOrCreateField(IPropertyField propertyField, Span<string> path)
		{
			foreach (var field in _entitySchemaAssemblage.Fields)
			{
				if (path.SequenceEqual(new ReadOnlySpan<string>(field.ModelPath)))
					return field;
			}

			var (fieldDefinition, fieldBuilder) = GetFieldDefinitionAndBuilder(propertyField);
			var newField = fieldBuilder.CreateAssemblage(path.ToArray());
			_entitySchemaAssemblage.AddField(newField);
			return newField;
		}

		private (EntityFieldDefinition Definition, IEntityFieldBuilder Builder) GetFieldDefinitionAndBuilder(IPropertyField propertyField)
		{
			//  reflection justification:
			//    building a schema is a rare occurance, using reflection here should make the use of generic types
			//    possible through the entire codebase while only executing slow reflection code this once
			var methodInfo = typeof(EntitySchemaBuilder<T>)
				.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
				.First(q => q.Name == nameof(GetFieldDefinitionAndBuilder) && q.IsGenericMethod)
				.MakeGenericMethod(propertyField.FieldType);
			return ((EntityFieldDefinition, IEntityFieldBuilder))methodInfo.Invoke(this, new object[] { propertyField });
		}

		private (EntityFieldDefinition Definition, IEntityFieldBuilder Builder) GetFieldDefinitionAndBuilder<TProperty>(PropertyField<TProperty> propertyField)
		{
			var fieldDefinition = _entitySchemaDefinition.For(propertyField);
			IEntityFieldBuilder builder;
			if (SqlTypeHelper.IsSqlPrimitiveType(propertyField.FieldType))
				builder = new SqlPrimitiveEntityFieldBuilder<TProperty, T>(fieldDefinition);
			else
				builder = new ObjectEntityFieldBuilder<TProperty, T>(fieldDefinition);
			return (fieldDefinition, builder);
		}
	}
}
