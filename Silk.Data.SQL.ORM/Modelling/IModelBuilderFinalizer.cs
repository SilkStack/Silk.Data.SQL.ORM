namespace Silk.Data.SQL.ORM.Modelling
{
	public interface IModelBuilderFinalizer
	{
		void FinalizeBuiltModel(Schema.Schema finalizingSchema);
	}
}
