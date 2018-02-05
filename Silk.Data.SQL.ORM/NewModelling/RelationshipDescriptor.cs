namespace Silk.Data.SQL.ORM.NewModelling
{
	/// <summary>
	/// Describes a relationship reference.
	/// </summary>
	public class RelationshipDescriptor
	{
		/// <summary>
		/// Gets the type of relationship referenced.
		/// </summary>
		public RelationshipType RelationshipType { get; }

		/// <summary>
		/// Gets the name (aka the alias) of the relationship referenced.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the schema of the related object.
		/// </summary>
		public IEntitySchema RelatedObjectSchema { get; }

		public RelationshipDescriptor(RelationshipType relationshipType, string name, IEntitySchema relatedObjectSchema)
		{
			RelationshipType = relationshipType;
			Name = name;
			RelatedObjectSchema = relatedObjectSchema;
		}
	}
}
