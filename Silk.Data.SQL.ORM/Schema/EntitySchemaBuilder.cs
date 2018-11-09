using Silk.Data.Modelling;
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
			var joins = new EntityFieldJoin[0];
			var indexes = new SchemaIndex[0];
			var mapping = default(Mapping);
			var entitySchema = new EntitySchema<T>(
				table, GetSchemaFields(),
				joins, indexes, mapping
				);
			return entitySchema;
		}

		private ISchemaField[] GetSchemaFields()
		{
			throw new NotImplementedException();
		}

		//private IEntityFieldOfEntity<T>[] GetEntityFields()
		//{
		//	var entityFields = new List<IEntityFieldOfEntity<T>>();
		//	EntityTypeVisitor.Visit(_entityTypeModel, Callback);
		//	return entityFields.ToArray();

		//	void Callback(IPropertyField propertyField, Span<string> path)
		//	{
		//		var fieldAssemblage = FindOrCreateField(propertyField, path);
		//		if (fieldAssemblage.Builder.BuildEntityField() is IEntityFieldOfEntity<T> entityField)
		//			entityFields.Add(entityField);
		//	}
		//}

		//private ProjectionField[] GetProjectionFields()
		//{
		//	var projectionFields = new List<ProjectionField>();
		//	EntityTypeVisitor.Visit(_entityTypeModel, Callback);
		//	return projectionFields.ToArray();

		//	void Callback(IPropertyField propertyField, Span<string> path)
		//	{
		//		var fieldAssemblage = FindOrCreateField(propertyField, path);
		//		var projectionField = fieldAssemblage.Builder.BuildProjectionField();
		//		if (projectionField != null)
		//			projectionFields.Add(projectionField);
		//	}
		//}

		private ISchemaFieldAssemblage FindOrCreateField(IPropertyField propertyField, Span<string> path)
		{
			foreach (var field in _entitySchemaAssemblage.Fields)
			{
				if (path.SequenceEqual(new ReadOnlySpan<string>(field.ModelPath)))
					return field;
			}

			var (fieldDefinition, fieldBuilder) = GetFieldDefinitionAndBuilder(propertyField, path);
			var newField = fieldBuilder.CreateAssemblage(path.ToArray());
			_entitySchemaAssemblage.AddField(newField);
			return newField;
		}

		private (SchemaFieldDefinition Definition, ISchemaFieldBuilder Builder) GetFieldDefinitionAndBuilder(IPropertyField propertyField, Span<string> path)
		{
			//  reflection justification:
			//    building a schema is a rare occurance, using reflection here should make the use of generic types
			//    possible through the entire codebase while only executing slow reflection code this once
			var methodInfo = typeof(EntitySchemaBuilder<T>)
				.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
				.First(q => q.Name == nameof(GetFieldDefinitionAndBuilder) && q.IsGenericMethod)
				.MakeGenericMethod(propertyField.FieldType);
			return ((SchemaFieldDefinition, ISchemaFieldBuilder))methodInfo.Invoke(this, new object[] { propertyField, path.ToArray() });
		}

		private (SchemaFieldDefinition Definition, ISchemaFieldBuilder Builder) GetFieldDefinitionAndBuilder<TProperty>(PropertyField<TProperty> propertyField, string[] path)
		{
			var fieldDefinition = _entitySchemaDefinition.For(propertyField);
			ISchemaFieldBuilder builder;
			if (SqlTypeHelper.IsSqlPrimitiveType(propertyField.FieldType))
				builder = new SqlPrimitiveSchemaFieldBuilder<TProperty, T>(_entitySchemaAssemblage, fieldDefinition);
			else
				builder = new ObjectEntityFieldBuilder<TProperty, T>(_entitySchemaAssemblage, _entitySchemaAssemblages, fieldDefinition);
			return (fieldDefinition, builder);
		}
	}
}
