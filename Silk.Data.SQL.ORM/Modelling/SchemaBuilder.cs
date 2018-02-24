using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using Silk.Data.SQL.ORM;
using Silk.Data.SQL.ORM.Modelling.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silk.Data.SQL.ORM.Modelling
{
	/// <summary>
	/// API used in <see cref="ISchemaConvention">ISchemaConvention</see> to design a data domain's schema.
	/// </summary>
	public abstract class SchemaBuilder
	{
		private readonly Dictionary<Type, EntityDefinition> _entityDefinitions
			= new Dictionary<Type, EntityDefinition>();

		private readonly Dictionary<ModelField, FieldOpinions> _fieldOpinions =
			new Dictionary<ModelField, FieldOpinions>();

		private readonly Stack<ContextStackEntry> _modelStack = new Stack<ContextStackEntry>();

		/// <summary>
		/// Gets an array of schema conventions being used to build the schema.
		/// </summary>
		public ISchemaConvention[] Conventions { get; }

		/// <summary>
		/// Gets an array of entity models that will be included in the schema.
		/// </summary>
		public TypedModel[] EntityModels { get; }

		/// <summary>
		/// Gets a value indicating if the schema has been altered.
		/// </summary>
		public bool WasAltered { get; protected set; }

		/// <summary>
		/// Gets a value indicating if the context stack is empty.
		/// </summary>
		public bool IsContextStackEmpty => _modelStack.Count == 0;

		/// <summary>
		/// Gets a value indicating if the current context represents the root model being examined.
		/// </summary>
		public bool IsAtContextRoot => _modelStack.Count < 2;

		protected SchemaBuilder(Dictionary<TypedModel,ModelOpinions> modelOpinions,
			ISchemaConvention[] schemaConventions)
		{
			Conventions = schemaConventions;

			EntityModels = modelOpinions.Keys.ToArray();

			foreach (var kvp in modelOpinions)
			{
				_entityDefinitions.Add(
					kvp.Key.DataType,
					new EntityDefinition { EntityModel = kvp.Key, TableName = kvp.Value.Name ?? kvp.Key.Name }
					);
				foreach (var field in kvp.Key.Fields)
				{
					_fieldOpinions.Add(field, kvp.Value.GetFieldOpinions(field));
				}
			}
		}

		/// <summary>
		/// Pushes a model onto the context for examining sub-models.
		/// </summary>
		public void PushModelOntoContext(TypedModel model, string name)
		{
			_modelStack.Push(new ContextStackEntry(model, name));
		}

		/// <summary>
		/// Pops a model off the context for examining sub-models.
		/// </summary>
		public TypedModel PopModelOffContext()
		{
			return _modelStack.Pop()?.Model;
		}

		/// <summary>
		/// Returns true when the provided type is defined as an entity type.
		/// </summary>
		public bool IsEntityType(Type type)
		{
			return EntityModels.Any(q => q.DataType == type);
		}

		/// <summary>
		/// Gets the <see cref="EntityDefinition"/> for a given type.
		/// </summary>
		/// <returns>The entity tyoe's definition or null if it's not present in the domain.</returns>
		public EntityDefinition GetEntityDefinition(Type entityType)
		{
			_entityDefinitions.TryGetValue(entityType, out var entityDefinition);
			return entityDefinition;
		}

		/// <summary>
		/// Defines a field for an entity type.
		/// </summary>
		/// <param name="entityModel"></param>
		/// <param name="name"></param>
		/// <param name="sqlDataType"></param>
		public void DefineField(TypedModel entityModel, string name,
			SqlDataType sqlDataType, Type clrType, ModelBinding binding,
			FieldOpinions fieldOpinions)
		{
			_entityDefinitions[entityModel.DataType].Fields.Add(
				FieldDefinition.SimpleMappedField(name, binding, fieldOpinions, sqlDataType, clrType)
				);
			WasAltered = true;
		}

		private string MakeContextName(string name)
		{
			if (IsAtContextRoot)
				return name;

			var ret = new StringBuilder();
			foreach (var entry in _modelStack.Reverse().Skip(1))
			{
				ret.Append($"{entry.Name}_");
			}
			ret.Append(name);
			return ret.ToString();
		}

		/// <summary>
		/// Defines a field for the current entity type in context.
		/// </summary>
		public void DefineFieldInContext(string name, SqlDataType sqlDataType,
			Type clrType, ModelBinding binding,
			FieldOpinions fieldOpinions)
		{
			if (IsContextStackEmpty)
				throw new InvalidOperationException("Context stack is empty.");
			var entityType = _modelStack.Last().Model.DataType; //  todo: optimize this? We don't need to enumerate each time, surely.
			_entityDefinitions[entityType].Fields.Add(
				FieldDefinition.SimpleMappedField(MakeContextName(name), binding, fieldOpinions, sqlDataType, clrType)
				);
			WasAltered = true;
		}

		/// <summary>
		/// Gets the existing definition for a field on an entity.
		/// </summary>
		/// <param name="entityModel"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public FieldDefinition GetDefinedField(TypedModel entityModel, string name)
		{
			return _entityDefinitions[entityModel.DataType].Fields.FirstOrDefault(q => q.Name == name);
		}

		/// <summary>
		/// Gets the existing definition for a field on the current context.
		/// </summary>
		public FieldDefinition GetDefinedFieldInContext(string name)
		{
			if (IsContextStackEmpty)
				throw new InvalidOperationException("Context stack is empty.");
			var entityType = _modelStack.Last().Model.DataType; //  todo: optimize this? We don't need to enumerate each time, surely.
			var contextName = MakeContextName(name);
			return _entityDefinitions[entityType].Fields.FirstOrDefault(q => q.Name == contextName);
		}

		/// <summary>
		/// Determines if a field is already defined.
		/// </summary>
		/// <param name="entityModel"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool IsFieldDefined(TypedModel entityModel, string name)
		{
			return _entityDefinitions[entityModel.DataType].Fields.Any(q => q.Name == name);
		}

		/// <summary>
		/// Determines if a field is already defined in the current context.
		/// </summary>
		public bool IsFieldDefinedInContext(string name)
		{
			if (IsContextStackEmpty)
				throw new InvalidOperationException("Context stack is empty.");
			var entityType = _modelStack.Last().Model.DataType; //  todo: optimize this? We don't need to enumerate each time, surely.
			var contextName = MakeContextName(name);
			return _entityDefinitions[entityType].Fields.Any(q => q.Name == contextName);
		}

		/// <summary>
		/// Gets the field opinions for a given model field.
		/// </summary>
		/// <param name="modelField"></param>
		/// <returns></returns>
		public FieldOpinions GetFieldOpinions(ModelField modelField)
		{
			if (_fieldOpinions.TryGetValue(modelField, out var opinions))
				return opinions;
			return FieldOpinions.Default;
		}

		/// <summary>
		/// Build the complete schema definition.
		/// </summary>
		/// <returns></returns>
		public SchemaDefinition BuildDefinition()
		{
			var schemaDefinition = new SchemaDefinition();
			schemaDefinition.Entities.AddRange(_entityDefinitions.Values);
			return schemaDefinition;
		}

		private string[] MakeContextPath(string fieldName)
		{
			if (IsAtContextRoot)
				return new [] { fieldName };

			var ret = new List<string>();
			foreach (var entry in _modelStack.Reverse().Skip(1))
			{
				ret.Add(entry.Name);
			}
			ret.Add(fieldName);
			return ret.ToArray();
		}

		public AssignmentBinding CreateAssignmentBindingInContext(BindingDirection bindingDirection,
			string modelFieldName, string sqlFieldName)
		{
			return new AssignmentBinding(bindingDirection, MakeContextPath(modelFieldName), new [] { MakeContextName(sqlFieldName) });
		}

		private class ContextStackEntry
		{
			public TypedModel Model { get; }
			public string Name { get; }

			public ContextStackEntry(TypedModel model, string name)
			{
				Model = model;
				Name = name;
			}
		}
	}
}
