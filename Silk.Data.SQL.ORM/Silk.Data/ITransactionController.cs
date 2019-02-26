using System.Threading.Tasks;

namespace Silk.Data
{
	public interface ITransactionController
	{
		bool AreEquivalentSharedControllers(ITransactionController transactionController);

		void Begin();
		Task BeginAsync();
		void Commit();
		void Rollback();
	}
}
