using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling
{
	/// <summary>
	/// API used in <see cref="ISchemaConvention">ISchemaConvention</see> to design a data domain's schema.
	/// </summary>
	public abstract class SchemaBuilder
	{
		private readonly Dictionary<Type, TableDefinition> _entityTableDefinitions
			= new Dictionary<Type, TableDefinition>();

		private readonly Dictionary<ModelField, FieldOpinions> _fieldOpinions =
			new Dictionary<ModelField, FieldOpinions>();

		/// <summary>
		/// Gets an array of entity models that will be included in the schema.
		/// </summary>
		public TypedModel[] EntityModels { get; }

		/// <summary>
		/// Gets a value indicating if the schema has been altered.
		/// </summary>
		public bool WasAltered { get; protected set; }

		protected SchemaBuilder(Dictionary<TypedModel,ModelOpinions> modelOpinions)
		{
			EntityModels = modelOpinions.Keys.ToArray();

			foreach (var kvp in modelOpinions)
			{
				_entityTableDefinitions.Add(
					kvp.Key.DataType,
					new TableDefinition { EntityType = kvp.Key.DataType, IsEntityTable = true, TableName = kvp.Value.Name ?? kvp.Key.Name }
					);
				foreach (var field in kvp.Key.Fields)
				{
					_fieldOpinions.Add(field, kvp.Value.GetFieldOpinions(field));
				}
			}
		}

		/// <summary>
		/// Defines a field for an entity type.
		/// </summary>
		/// <param name="entityModel"></param>
		/// <param name="name"></param>
		/// <param name="sqlDataType"></param>
		public void DefineField(TypedModel entityModel, string name, SqlDataType sqlDataType,
			ModelBinding binding, FieldOpinions fieldOpinions)
		{
			_entityTableDefinitions[entityModel.DataType].Fields.Add(
				new ViewFieldDefinition(name, binding)
				);
			WasAltered = true;
		}

		/// <summary>
		/// Determines if a field is already defined.
		/// </summary>
		/// <param name="entityModel"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool IsFieldDefined(TypedModel entityModel, string name)
		{
			return _entityTableDefinitions[entityModel.DataType].Fields.Any(q => q.Name == name);
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
	}
}
