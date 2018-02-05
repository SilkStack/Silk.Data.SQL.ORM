using Silk.Data.Modelling;

namespace Silk.Data.SQL.ORM.NewModelling
{
	public interface IEntitySchema : IView<IDataField>
	{
	}

	public interface IEntitySchema<T> : IView<IDataField, T>
		where T : new()
	{
	}
}
