using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Modelling.Conventions;
using System.Linq;

namespace Silk.Data.SQL.ORM
{
	/// <summary>
	/// Describes the types, models and resource loaders of a data domain.
	/// </summary>
	public class DataDomain
	{
		private static ViewConvention[] _defaultViewConventions = new ViewConvention[]
		{
			new CopySupportedSQLTypesConvention(),
			new IdIsPrimaryKeyConvention()
		};

		public ViewConvention[] ViewConventions { get; }

		public DataDomain() :
			this(_defaultViewConventions)
		{
		}

		public DataDomain(ViewConvention[] viewConventions)
		{
			ViewConventions = viewConventions;
		}

		/// <summary>
		/// Creates a data model using knowledge from the data domain.
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <returns></returns>
		public DataModel<TSource> CreateDataModel<TSource>()
			where TSource : new()
		{
			var model = TypeModeller.GetModelOf<TSource>();
			return model.CreateView(viewDefinition => new DataModel<TSource>(viewDefinition.Name,
					model, DataField.FromDefinitions(viewDefinition.UserData.OfType<TableDefinition>(), viewDefinition.FieldDefinitions).ToArray(),
					viewDefinition.ResourceLoaders.ToArray()), ViewConventions);
		}

		/// <summary>
		/// Creates a data model using knowledge from the data domain.
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <returns></returns>
		public DataModel<TSource, TView> CreateDataModel<TSource, TView>()
			where TSource : new()
			where TView : new()
		{
			var model = TypeModeller.GetModelOf<TSource>();
			return model.CreateView(viewDefinition => new DataModel<TSource, TView>(viewDefinition.Name,
					model, DataField.FromDefinitions(viewDefinition.UserData.OfType<TableDefinition>(), viewDefinition.FieldDefinitions).ToArray(),
					viewDefinition.ResourceLoaders.ToArray()),
				typeof(TView), ViewConventions);
		}
	}
}
