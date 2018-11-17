namespace Silk.Data.SQL.ORM.Schema
{
	public enum FieldType
	{
		/// <summary>
		/// A field storing an SQL primitive as part of the entity schema.
		/// </summary>
		StoredField,
		/// <summary>
		/// A field stored elsewhere as part of a joined entity.
		/// </summary>
		JoinedField,
		/// <summary>
		/// A field storing an SQL primitive that references another joined entity.
		/// </summary>
		ReferenceField
	}
}
