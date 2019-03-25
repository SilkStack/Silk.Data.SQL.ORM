using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.Mapping;
using Silk.Data.SQL.ORM.Modelling;

namespace Silk.Data.SQL.ORM.Schema
{
	public interface IEntityView
	{
	}

	/// <summary>
	/// Transcribes values from a model.
	/// </summary>
	public interface IEntityView<TView> : IEntityView
		where TView : class
	{
		IIntersection<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>
			ClassToEntityIntersection { get; }

		IIntersection<EntityModel, EntityField, TypeModel, PropertyInfoField>
			EntityToClassIntersection { get; }

		IMapping<EntityModel, EntityField, TypeModel, PropertyInfoField> Mapping { get; }
	}
}
