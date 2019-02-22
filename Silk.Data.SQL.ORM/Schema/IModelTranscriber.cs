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

		IReadOnlyList<IntersectedFieldWriter<TView>> FieldWriters { get; }
	}
}
