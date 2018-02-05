using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Modelling
{
	/// <summary>
	/// Defines the entire database schema.
	/// </summary>
	public class SchemaDefinition
	{
		/// <summary>
		/// Gets or sets a collection of defined entities.
		/// </summary>
		public List<EntityDefinition> Entities { get; set; }
			= new List<EntityDefinition>();
	}
}
