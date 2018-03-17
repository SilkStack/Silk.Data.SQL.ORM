using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Modelling;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class EntitySchemaOptions
	{
		public EntityModelTransformer ModelTransformer { get; protected set; }

		public abstract bool PerformTransformationPass();

		public abstract EntityModel GetEntityModel();

		public EntityFieldOptions GetFieldOptions(IField field)
		{
			return null;
		}
	}

	public class EntitySchemaOptions<T> : EntitySchemaOptions
	{
		public TypeModel<T> EntityTypeModel { get; }

		public new EntityModelTransformer<T> ModelTransformer { get; }

		public EntitySchemaOptions(SchemaBuilder schemaBuilder)
		{
			EntityTypeModel = TypeModel.GetModelOf<T>();
			ModelTransformer = new EntityModelTransformer<T>(this, schemaBuilder);
			base.ModelTransformer = ModelTransformer;
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

	public abstract class EntityFieldOptions
	{
		public string ConfiguredColumnName { get; protected set; }
		public bool IsPrimaryKey { get; protected set; }
		public bool IsAutoGenerate { get; protected set; }
		public bool IsIndex { get; protected set; }
		public int? ConfiguredPrecision { get; protected set; }
		public int? ConfiguredScale { get; protected set; }
		public int? ConfiguredDataLength { get; protected set; }
	}
}
