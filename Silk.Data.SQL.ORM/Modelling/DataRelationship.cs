namespace Silk.Data.SQL.ORM.Modelling
{
	public class DataRelationship
    {
		public DataField ForeignField { get; }
		public EntityModel ForeignModel { get; }
		public RelationshipType RelationshipType { get; }

		public DataRelationship(DataField foreignField, EntityModel foreignModel,
			RelationshipType relationshipType)
		{
			ForeignField = foreignField;
			ForeignModel = foreignModel;
			RelationshipType = relationshipType;
		}
    }
}
