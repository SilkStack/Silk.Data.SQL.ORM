using System;
using System.Collections.Generic;
using System.Text;

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
		EntitySchema BuildSchema(PartialEntitySchemaCollection partialEntities);
	}

	/// <summary>
	/// Builds the schema components for entities of type T.
	/// </summary>
	public class EntitySchemaBuilder<T> : IEntitySchemaBuilder
		where T : class
	{
		private readonly EntitySchemaDefinition<T> _entitySchemaDefinition;
		private EntitySchemaAssemblage<T> _entitySchemaAssemblage;

		public EntitySchemaBuilder(EntitySchemaDefinition<T> entitySchemaDefinition)
		{
			_entitySchemaDefinition = entitySchemaDefinition;
		}

		public IEntitySchemaAssemblage CreateAssemblage()
		{
			_entitySchemaAssemblage = new EntitySchemaAssemblage<T>(
				!string.IsNullOrWhiteSpace(_entitySchemaDefinition.TableName) ?
					_entitySchemaDefinition.TableName :
					typeof(T).Name
				);
			return _entitySchemaAssemblage;
		}

		public EntitySchema BuildSchema(PartialEntitySchemaCollection partialEntities)
		{
			throw new NotImplementedException();
		}

		public bool DefineNewSchemaFields(PartialEntitySchemaCollection partialEntities)
		{
			throw new NotImplementedException();
		}
	}
}
