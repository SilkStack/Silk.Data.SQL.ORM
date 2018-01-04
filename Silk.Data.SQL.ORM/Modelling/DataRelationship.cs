namespace Silk.Data.SQL.ORM.Modelling
{
	public class DataRelationship
    {
		public EntityModel ForeignModel { get; }
		public EntityModel ProjectedModel { get; }
		public RelationshipType RelationshipType { get; }

		public DataRelationship(EntityModel foreignModel,
			RelationshipType relationshipType)
		{
			ForeignModel = foreignModel;
			RelationshipType = relationshipType;
		}

		public DataRelationship(EntityModel foreignModel,
			EntityModel projectedModel,
			RelationshipType relationshipType) :
			this(foreignModel, relationshipType)
		{
			ProjectedModel = projectedModel;
		}
	}
}
