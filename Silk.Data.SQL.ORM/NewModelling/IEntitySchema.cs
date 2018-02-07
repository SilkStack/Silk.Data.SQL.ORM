using Silk.Data.Modelling;

namespace Silk.Data.SQL.ORM.NewModelling
{
	public interface IEntitySchema : IView<IDataField>
	{
		/// <summary>
		/// Gets the entities main storage table.
		/// </summary>
		Table EntityTable { get; }
	}

	public interface IEntitySchema<T> : IEntitySchema, IView<IDataField, T>
		where T : new()
	{
	}
}
