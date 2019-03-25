using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.Mapping;
using Silk.Data.SQL.ORM.Modelling;

namespace Silk.Data.SQL.ORM.Schema
{
	public class EntityViewFactory
	{
		public static IEntityView<TView> Create<TEntity, TView>(
			ClassToEntityIntersectionAnalyzer classToEntityAnalyzer,
			IIntersectionAnalyzer<EntityModel, EntityField, TypeModel, PropertyInfoField> entityToTypeAnalyzer,
			EntityModel<TEntity> entityModel
			)
			where TEntity : class
			where TView : class
		{
			var viewTypeModel = TypeModel.GetModelOf<TView>();

			return new EntityView<TView>(
				classToEntityAnalyzer.CreateIntersection(viewTypeModel, entityModel),
				entityToTypeAnalyzer.CreateIntersection(entityModel, viewTypeModel)
				);
		}

		private class EntityView<TView> : IEntityView<TView>
			where TView : class
		{
			public IIntersection<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField> ClassToEntityIntersection { get; }

			public IIntersection<EntityModel, EntityField, TypeModel, PropertyInfoField> EntityToClassIntersection { get; }

			public IMapping<EntityModel, EntityField, TypeModel, PropertyInfoField> Mapping { get; }

			public EntityView(
				IIntersection<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField> classToEntityModelIntersection,
				IIntersection<EntityModel, EntityField, TypeModel, PropertyInfoField> entityToClassIntersection
				)
			{
				ClassToEntityIntersection = classToEntityModelIntersection;
				EntityToClassIntersection = entityToClassIntersection;

				var mappingFactory = new DefaultMappingFactory<EntityModel, EntityField, TypeModel, PropertyInfoField>();
				Mapping = mappingFactory.CreateMapping(entityToClassIntersection);
			}
		}
	}
}
