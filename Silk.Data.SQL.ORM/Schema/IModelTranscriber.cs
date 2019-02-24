using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Schema
{
	public interface IModelTranscriber
	{
	}

	/// <summary>
	/// Transcribes values from a model.
	/// </summary>
	public interface IModelTranscriber<TView> : IModelTranscriber
		where TView : class
	{
		IIntersection<TypeModel, PropertyInfoField, EntityModel, EntityField>
			TypeModelToEntityModelIntersection { get; }

		IIntersection<EntityModel, EntityField, TypeModel, PropertyInfoField>
			EntityModelToTypeModelIntersection { get; }

		/// <summary>
		/// Models how an object type maps onto the storage model.
		/// Useful for reading storage fields from entity and view types.
		/// </summary>
		IReadOnlyList<EntityModelHelper<TView>> ObjectToSchemaHelpers { get; }

		/// <summary>
		/// Models how the storage model maps onto an object type.
		/// Useful for reading entity/view data from result rows onto an instance.
		/// </summary>
		IReadOnlyList<TypeModelHelper<TView>> SchemaToTypeHelpers { get; }
	}
}
