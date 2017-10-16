using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling
{
	public static class TypedModelExtensions
	{
		public static DataModel<TSource,TView> CreateDataModel<TSource, TView>(this TypedModel<TSource> model,
			params ViewConvention[] viewConventions)
		{
			return model.CreateView(viewDefinition => new DataModel<TSource,TView>(viewDefinition.Name,
					model, DataField.FromDefinitions(viewDefinition.FieldDefinitions).ToArray(),
					viewDefinition.ResourceLoaders.ToArray()),
				viewConventions);
		}

		public static DataModel<TSource, TView> CreateDataModel<TSource, TView>(
			this TypedModel<TSource>.Modeller<TView> modeller,
			params ViewConvention[] viewConventions)
		{
			return modeller.Model.CreateDataModel<TSource, TView>(viewConventions);
		}
	}
}
