using Silk.Data.Modelling;
using Silk.Data.Modelling.ResourceLoaders;
using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.ORM.Modelling
{
	public abstract class DataModel : IView<DataField>
	{
		public DataField[] Fields { get; }

		public string Name { get; }

		public Model Model { get; }

		public IResourceLoader[] ResourceLoaders { get; }

		IViewField[] IView.Fields => Fields;

		public DataModel(string name, Model model, DataField[] fields,
			IResourceLoader[] resourceLoaders)
		{
			Name = name;
			Model = model;
			Fields = fields;
			ResourceLoaders = resourceLoaders;
		}
	}

	public class DataModel<TSource, TView> : DataModel, IView<DataField, TSource, TView>
	{
		public new TypedModel<TSource> Model { get; }

		public DataModel(string name, TypedModel<TSource> model, DataField[] fields, IResourceLoader[] resourceLoaders)
			: base(name, model, fields, resourceLoaders)
		{
			Model = model;
		}

		public QueryExpression Insert(TSource source)
		{
			return null;
		}
	}
}
