using Silk.Data.Modelling;
using Silk.Data.Modelling.ResourceLoaders;
using Silk.Data.SQL.ORM.NewModelling;

namespace Silk.Data.SQL.ORM
{
	public abstract class Projection : IEntitySchema
	{
		public Table EntityTable => throw new System.NotImplementedException();

		public IDataField[] Fields => throw new System.NotImplementedException();

		public string Name => throw new System.NotImplementedException();

		public Model Model => throw new System.NotImplementedException();

		public IResourceLoader[] ResourceLoaders => throw new System.NotImplementedException();

		IViewField[] IView.Fields => throw new System.NotImplementedException();
	}

	public class Projection<T> : Projection, IEntitySchema<T>
		where T : new()
	{
		public new TypedModel<T> Model => throw new System.NotImplementedException();
	}
}
