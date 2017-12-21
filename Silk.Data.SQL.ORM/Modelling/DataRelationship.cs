namespace Silk.Data.SQL.ORM.Modelling
{
	public class DataRelationship
    {
		public EntityModel ForeignModel { get; }
		public RelationshipType RelationshipType { get; }

		public DataRelationship(EntityModel foreignModel,
			RelationshipType relationshipType)
		{
			ForeignModel = foreignModel;
			RelationshipType = relationshipType;
		}
    }
}
