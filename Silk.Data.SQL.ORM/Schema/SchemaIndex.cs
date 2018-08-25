namespace Silk.Data.SQL.ORM.Schema
{
	public class SchemaIndex
	{
		public string Name { get; }
		public bool HasUniqueConstraint { get; }
		public EntityField[] EntityFields { get; }

		public SchemaIndex(string name, bool hasUniqueConstraint, EntityField[] entityFields)
		{
			Name = name;
			HasUniqueConstraint = hasUniqueConstraint;
			EntityFields = entityFields;
		}
	}
}
