using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Modelling;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class EntitySchemaOptions
	{
		public abstract bool PerformTransformationPass();

		public abstract EntityModel GetEntityModel();
	}

	public class EntitySchemaOptions<T> : EntitySchemaOptions
	{
		public TypeModel<T> EntityTypeModel { get; }

		public EntityModelTransformer<T> ModelTransformer { get; }

		public EntitySchemaOptions()
		{
			EntityTypeModel = TypeModel.GetModelOf<T>();
			ModelTransformer = new EntityModelTransformer<T>();
		}

		public override bool PerformTransformationPass()
		{
			ModelTransformer.FieldsAdded = false;
			EntityTypeModel.Transform(ModelTransformer);
			return ModelTransformer.FieldsAdded;
		}

		public override EntityModel GetEntityModel()
		{
			return ModelTransformer.GetEntityModel();
		}
	}
}
