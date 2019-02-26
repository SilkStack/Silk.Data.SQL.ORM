using System.Threading.Tasks;

namespace Silk.Data
{
	/// <summary>
	/// A deferred task waiting to be executed.
	/// </summary>
	public interface IDeferred
	{
		/// <summary>
		/// Create a new instance of the preferred transaction controller implemenation.
		/// Note: this might not be the transaction controller the task will end up using.
		/// </summary>
		/// <returns></returns>
		ITransactionController GetTransactionControllerImplementation();

		/// <summary>
		/// Set the transaction controller to use when executing.
		/// </summary>
		/// <param name="transactionController"></param>
		void SetSharedTransactionController(ITransactionController transactionController);

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
