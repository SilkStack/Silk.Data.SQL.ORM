namespace Silk.Data
{
	/// <summary>
	/// Contains all the requisite information to reference a specific instance of T.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IEntityReference<T>
	{
		T AsEntity();
	}
}
