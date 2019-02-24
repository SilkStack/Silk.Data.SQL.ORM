using System.Threading.Tasks;

namespace Silk.Data
{
	/// <summary>
	/// A deferred task waiting to be executed.
	/// </summary>
	public interface IDeferred
	{
		/// <summary>
		/// Execute the deferred task.
		/// </summary>
		void Execute();

		/// <summary>
		/// Execute the deferred task asynchronously.
		/// </summary>
		/// <returns></returns>
		Task ExecuteAsync();
	}
}
