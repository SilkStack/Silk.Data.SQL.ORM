using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;

namespace Silk.Data.SQL.ORM.Modelling
{
	public static class TypedModelExtensions
	{
		public static DataModel<TSource,TView> CreateDataModel<TSource, TView>(this TypedModel<TSource> model,
			params ViewConvention[] viewConventions)
		{
			return null;
		}

		public static DataModel<TSource, TView> CreateDataModel<TSource, TView>(
			this TypedModel<TSource>.Modeller<TView> modeller,
			params ViewConvention[] viewConventions)
		{
			return modeller.Model.CreateDataModel<TSource, TView>(viewConventions);
		}
	}
}
