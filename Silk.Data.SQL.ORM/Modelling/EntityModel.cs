using System;
using System.Collections.Generic;
using System.Linq;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.SQL.ORM.Modelling.Binding;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Modelling
{
	public abstract class EntityModel : ProjectionModel
	{
		public Type EntityType { get; }
		public Table[] JunctionTables { get; protected set; }

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

		public void FinalizeBuiltModel(Schema.Schema finalizingSchema, List<Table> tables)
		{
			var mappingBuilder = new MappingBuilder(this, TypeModel.GetModelOf<T>());
			mappingBuilder.AddConvention(CreateInstanceAsNeeded.Instance);
			mappingBuilder.AddConvention(CreateEmbeddedInstanceUsingNotNullColumn.Instance);
			mappingBuilder.AddConvention(CreateSingleRelatedInstanceWhenPresent.Instance);
			mappingBuilder.AddConvention(CopyValueFields.Instance);

			Mapping = mappingBuilder.BuildMapping();

			JunctionTables = Fields.OfType<IManyRelatedObjectField>()
				.Select(q => q.JunctionTable).ToArray();
		}
	}
}
