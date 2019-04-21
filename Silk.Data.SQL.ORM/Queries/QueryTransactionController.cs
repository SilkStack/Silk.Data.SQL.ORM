using Silk.Data.SQL.Providers;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public class QueryTransactionController : ITransactionController
	{
		private readonly IDataProvider _dataProvider;
		private ITransaction _transaction;

		public ITransaction Transaction => _transaction;

		public QueryTransactionController(IDataProvider dataProvider)
		{
			_dataProvider = dataProvider;
		}

		public void Begin()
		{
			_transaction = _dataProvider.CreateTransaction();
		}

		public async Task BeginAsync()
		{
			_transaction = await _dataProvider.CreateTransactionAsync();
		}

		public void Commit()
		{
			_transaction.Commit();
		}

		public void Rollback()
		{
			_transaction.Rollback();
		}

		public bool AreEquivalentSharedControllers(ITransactionController transactionController)
		{
			var queryTransactionController = transactionController as QueryTransactionController;
			if (queryTransactionController == null)
				return false;

			return ReferenceEquals(_dataProvider, queryTransactionController._dataProvider);
		}

		public void Dispose()
		{
			_transaction.Dispose();
		}
	}
}
