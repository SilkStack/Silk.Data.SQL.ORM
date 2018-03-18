using System;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.SQL.ORM.Modelling.Binding;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Modelling
{
	public abstract class EntityModel : ProjectionModel
	{
		public Type EntityType { get; }

		public EntityModel(Type entityType, IEntityField[] fields, Table entityTable) :
			base(fields, entityTable)
		{
			EntityType = entityType;
		}

		public ProjectionModel GetProjection<TProjection>()
		{
			return null;
		}
	}

	public class EntityModel<T> : EntityModel, IModelBuilderFinalizer
	{
		public EntityModel(IEntityField[] fields, Table entityTable) :
			base(typeof(T), fields, entityTable)
		{
		}

		public void FinalizeBuiltModel(Schema.Schema finalizingSchema)
		{
			var mappingBuilder = new MappingBuilder(this, TypeModel.GetModelOf<T>());
			mappingBuilder.AddConvention(CreateInstanceAsNeeded.Instance);
			mappingBuilder.AddConvention(CreateInstancesOfPropertiesAsNeeded.Instance);
			mappingBuilder.AddConvention(CopyValueFields.Instance);

			Mapping = mappingBuilder.BuildMapping();
		}
	}
}
