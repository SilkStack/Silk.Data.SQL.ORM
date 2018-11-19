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

		Dictionary<ISchemaField, FieldOperations> BuildFieldOperations();
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
		private (ISchemaField<T> Field, ISchemaFieldAssemblage<T> Assemblage, FieldOperations<T> Operations)[] _builtFields;
		private int _joinCount;
		private readonly List<(ISchemaField<T>, FieldOperations<T>)> _fieldOperationsPairs
			= new List<(ISchemaField<T>, FieldOperations<T>)>();

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

				FindOrCreateField(propertyField, path, null);
			}
		}

		public EntitySchema BuildSchema()
		{
			_builtFields = BuildSchemaFields();
			var fields = _builtFields.Select(q => q.Field).ToArray();
			var table = new Table(_entitySchemaAssemblage.TableName,
				fields.Select(q => q.Column).ToArray());
			var joins = new EntityFieldJoin[0];
			var indexes = GetSchemaIndexes(table, fields);
			var mapping = default(Mapping);
			var entitySchema = new EntitySchema<T>(
				table, fields,
				joins, indexes, mapping
				);
			return entitySchema;
		}

		public Dictionary<ISchemaField, FieldOperations> BuildFieldOperations()
		{
			if (_builtFields == null)
				throw new InvalidOperationException("Schema is not yet built, call BuildSchema() before calling BuildFieldOperations().");
			return _builtFields.Select(q => new
			{
				Field = q.Field,
				Operations = q.Operations
			}).ToDictionary(q => (ISchemaField)q.Field, q => (FieldOperations)q.Operations);
		}

		private SchemaIndex[] GetSchemaIndexes(Table table, ISchemaField[] fields)
		{
			var indexes = new List<SchemaIndex>();
			foreach (var indexBuilder in _entitySchemaDefinition.GetIndexBuilders())
			{
				indexes.Add(indexBuilder.Build(table, fields));
			}
			return indexes.ToArray();
		}

		private (ISchemaField<T> Field, ISchemaFieldAssemblage<T> Assemblage, FieldOperations<T> Operations)[] BuildSchemaFields()
		{
			var fieldStack = new Stack<ISchemaField<T>>();
			var joinStack = new Stack<EntityFieldJoin>();

			var schemaFields = new List<(ISchemaField<T> Field, ISchemaFieldAssemblage<T> Assemblage, FieldOperations<T> Operations)>();
			EntityTypeVisitor.Visit(_entityTypeModel, Callback);
			return schemaFields.ToArray();

			void Callback(IPropertyField propertyField, Span<string> path)
			{
				var fieldAssemblage = FindOrCreateField(propertyField, path, joinStack.LastOrDefault());
				while (fieldStack.Count >= path.Length)
				{
					fieldStack.Pop();
					joinStack.Pop();
				}
				var builtFields = fieldAssemblage.Builder.Build(fieldStack)
					.ToArray();

				_fieldOperationsPairs.AddRange(builtFields);

				foreach (var builtFieldPair in builtFields)
				{
					var builtField = builtFieldPair.Field;
					fieldStack.Push(builtField);

					if (_entitySchemaAssemblages.Any(q => q.EntityType == propertyField.FieldType))
					{
						joinStack.Push(CreateJoin(fieldAssemblage));
					}
					else
					{
						joinStack.Push(joinStack.LastOrDefault());
					}

					schemaFields.Add((builtField, fieldAssemblage, builtFieldPair.Operations));
				}
			}
		}

		private EntityFieldJoin CreateJoin(ISchemaFieldAssemblage localFieldAssemblage)
		{
			EntityFieldJoin[] dependencyJoins;
			if (localFieldAssemblage.Join == null)
				dependencyJoins = new EntityFieldJoin[0];
			else
				dependencyJoins = new[] { localFieldAssemblage.Join };

			var foreignSchemaAssemblage = _entitySchemaAssemblages.First(q =>
				q.EntityType == localFieldAssemblage.FieldDefinition.ModelField.FieldType
				);

			return new EntityFieldJoin(
				foreignSchemaAssemblage.TableName,
				$"__join_table_{++_joinCount}",
				localFieldAssemblage.Join?.TableName ?? _entitySchemaAssemblage.TableName,
				new string[0],
				foreignSchemaAssemblage.Fields
					.Where(q => q.FieldDefinition.IsPrimaryKey)
					.Select(q => q.Column.ColumnName)
					.ToArray(),
				null,
				dependencyJoins);
		}

		private ISchemaFieldAssemblage<T> FindOrCreateField(IPropertyField propertyField, Span<string> path, EntityFieldJoin join)
		{
			foreach (var field in _entitySchemaAssemblage.Fields)
			{

				if (path.SequenceEqual(new ReadOnlySpan<string>(field.ModelPath)))
					return field;
			}

			var (fieldDefinition, fieldBuilder) = GetFieldDefinitionAndBuilder(propertyField, path);
			var newField = fieldBuilder.CreateAssemblage(path.ToArray(), join);
			_entitySchemaAssemblage.AddField(newField);
			return newField;
		}

		private (SchemaFieldDefinition Definition, ISchemaFieldBuilder<T> Builder) GetFieldDefinitionAndBuilder(IPropertyField propertyField, Span<string> path)
		{
			//  reflection justification:
			//    building a schema is a rare occurance, using reflection here should make the use of generic types
			//    possible through the entire codebase while only executing slow reflection code this once

			if (SqlTypeHelper.IsSqlPrimitiveType(propertyField.FieldType))
			{
				var methodInfo = typeof(EntitySchemaBuilder<T>)
					.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
					.First(q => q.Name == nameof(GetValueFieldDefinitionAndBuilder) && q.IsGenericMethod)
					.MakeGenericMethod(propertyField.FieldType);
				return ((SchemaFieldDefinition, ISchemaFieldBuilder<T>))methodInfo.Invoke(this, new object[] { propertyField, path.ToArray() });
			}
			else
			{
				var methodInfo = typeof(EntitySchemaBuilder<T>)
					.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
					.First(q => q.Name == nameof(GetObjectFieldDefinitionAndBuilder) && q.IsGenericMethod)
					.MakeGenericMethod(propertyField.FieldType);
				return ((SchemaFieldDefinition, ISchemaFieldBuilder<T>))methodInfo.Invoke(this, new object[] { propertyField, path.ToArray() });
			}
		}

		private (SchemaFieldDefinition Definition, ISchemaFieldBuilder<T> Builder) GetValueFieldDefinitionAndBuilder<TProperty>(PropertyField<TProperty> propertyField, string[] path)
		{
			var fieldDefinition = _entitySchemaDefinition.For(propertyField);
			ISchemaFieldBuilder<T> builder;
			builder = new SqlPrimitiveSchemaFieldBuilder<TProperty, T>(_entitySchemaAssemblage, fieldDefinition);
			return (fieldDefinition, builder);
		}

		private (SchemaFieldDefinition Definition, ISchemaFieldBuilder<T> Builder) GetObjectFieldDefinitionAndBuilder<TProperty>(PropertyField<TProperty> propertyField, string[] path)
			where TProperty : class
		{
			var fieldDefinition = _entitySchemaDefinition.For(propertyField);
			ISchemaFieldBuilder<T> builder;
			builder = new ObjectEntityFieldBuilder<TProperty, T>(_entitySchemaAssemblage, _entitySchemaAssemblages, fieldDefinition);
			return (fieldDefinition, builder);
		}
	}
}
