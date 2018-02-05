using Silk.Data.Modelling;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Modelling
{
	/// <summary>
	/// Describes how an entity will be stored.
	/// </summary>
	public class EntityDefinition
	{
		/// <summary>
		/// Gets or sets the name of the entity's table.
		/// </summary>
		public string TableName { get; set; }
		/// <summary>
		/// Gets or sets the entity model.
		/// </summary>
		public TypedModel EntityModel { get; set; }
		/// <summary>
		/// Gets or sets the entity fields.
		/// </summary>
		public List<FieldDefinition> Fields { get; } = new List<FieldDefinition>();
	}
}
