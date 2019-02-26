using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silk.Data
{
	public static class DeferredCollectionExtensions
	{
		public static void Execute(this IEnumerable<IDeferred> deferredTasks)
			=> deferredTasks.ToExecutor().Execute();

		public static Task ExecuteAsync(this IEnumerable<IDeferred> deferredTasks)
			=> deferredTasks.ToExecutor().ExecuteAsync();

		public static void ExecuteInTransaction(this IEnumerable<IDeferred> deferredTasks)
		{
			var transaction = new Transaction();
			try
			{
				transaction.Execute(deferredTasks);
				transaction.Commit();
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
		}

		public static async Task ExecuteInTransactionAsync(this IEnumerable<IDeferred> deferredTasks)
		{
			var transaction = new Transaction();
			try
			{
				await transaction.ExecuteAsync(deferredTasks);
				transaction.Commit();
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
		}

		public static DeferredExecutor ToExecutor(this IEnumerable<IDeferred> deferredTasks)
		{
			var executor = new DeferredExecutor();

			foreach (var task in deferredTasks)
			{
				executor.Add(task);
			}

			return executor;
		}
	}
}
