namespace Silk.Data.SQL.ORM
{
	public enum RelationshipType
	{
		/// <summary>
		/// Many objects referring to a single foreign object.
		/// </summary>
		ManyToOne,
		/// <summary>
		/// Many objects referring to any number of foreign objects (through a relationship table).
		/// </summary>
		ManyToMany,
	}
}
