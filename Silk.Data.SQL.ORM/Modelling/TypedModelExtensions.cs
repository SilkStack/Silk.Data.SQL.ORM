using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;
using Silk.Data.SQL.ORM.Modelling.Conventions;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling
{
	public static class TypedModelExtensions
	{
		private static ViewConvention[] _defaultViewConventions = new ViewConvention[]
		{
			new CopySupportedSQLTypesConvention(),
			new IdIsPrimaryKeyConvention()
		};

		public static DataModel<TSource> CreateDataModel<TSource>(this TypedModel<TSource> model,
			params ViewConvention[] viewConventions)
			where TSource : new()
		{
			if (viewConventions == null || viewConventions.Length == 0)
				viewConventions = _defaultViewConventions;
			return model.CreateView(viewDefinition => new DataModel<TSource>(viewDefinition.Name,
					model, DataField.FromDefinitions(viewDefinition.UserData.OfType<TableDefinition>(), viewDefinition.FieldDefinitions).ToArray(),
					viewDefinition.ResourceLoaders.ToArray()), viewConventions);
		}

		public static DataModel<TSource,TView> CreateDataModel<TSource, TView>(this TypedModel<TSource> model,
			params ViewConvention[] viewConventions)
			where TSource : new()
			where TView : new()
		{
			if (viewConventions == null || viewConventions.Length == 0)
				viewConventions = _defaultViewConventions;
			return model.CreateView(viewDefinition => new DataModel<TSource,TView>(viewDefinition.Name,
					model, DataField.FromDefinitions(viewDefinition.UserData.OfType<TableDefinition>(), viewDefinition.FieldDefinitions).ToArray(),
					viewDefinition.ResourceLoaders.ToArray()),
				typeof(TView), viewConventions);
		}

		public static DataModel<TSource, TView> CreateDataModel<TSource, TView>(
			this TypedModel<TSource>.Modeller<TView> modeller,
			params ViewConvention[] viewConventions)
			where TSource : new()
			where TView : new()
		{
			return modeller.Model.CreateDataModel<TSource, TView>(viewConventions);
		}
	}
}
