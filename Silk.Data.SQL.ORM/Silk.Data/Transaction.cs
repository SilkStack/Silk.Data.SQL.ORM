using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data
{
	public class Transaction
	{
		private readonly List<ITransactionController> _transactionControllers
			= new List<ITransactionController>();

		private IEnumerable<ITransactionController> GetNewTransactionControllers(DeferredExecutor executor)
		{
			var result = new List<ITransactionController>();

			foreach (var task in executor)
			{
				var controller = task.GetTransactionControllerImplementation();

				var existingController = _transactionControllers.FirstOrDefault(
					q => q.AreEquivalentSharedControllers(controller)
					);
				if (existingController == null)
					existingController = result.FirstOrDefault(
						q => q.AreEquivalentSharedControllers(controller)
						);

				if (existingController != null)
				{
					task.SetSharedTransactionController(existingController);
					continue;
				}

				task.SetSharedTransactionController(controller);
				result.Add(controller);
			}

			return result;
		}

		public void Execute(IDeferable deferable)
			=> Execute(new[] { deferable.Defer() });

		public Task ExecuteAsync(IDeferable deferable)
			=> ExecuteAsync(new[] { deferable.Defer() });

		public T Execute<T>(IDeferable<T> deferable)
		{
			Execute(new[] { deferable.Defer(out var result) });
			return result.Result;
		}

		public async Task<T> ExecuteAsync<T>(IDeferable<T> deferable)
		{
			await ExecuteAsync(new[] { deferable.Defer(out var result) });
			return result.Result;
		}

		public void Execute(IEnumerable<IDeferred> deferredTasks)
		{
			var executor = deferredTasks.ToExecutor();
			foreach (var controller in GetNewTransactionControllers(executor))
			{
				controller.Begin();
				_transactionControllers.Add(controller);
			}
			executor.Execute();
		}

		public async Task ExecuteAsync(IEnumerable<IDeferred> deferredTasks)
		{
			var executor = deferredTasks.ToExecutor();
			foreach (var controller in GetNewTransactionControllers(executor))
			{
				await controller.BeginAsync();
				_transactionControllers.Add(controller);
			}
			await executor.ExecuteAsync();
		}

		public void Commit()
		{
			foreach (var controller in _transactionControllers)
				controller.Commit();
		}

		public void Rollback()
		{
			var exceptionCollection = new List<Exception>();
			foreach (var controller in _transactionControllers)
			{
				try
				{
					controller.Rollback();
				}
				catch(Exception ex)
				{
					exceptionCollection.Add(ex);
				}
			}

			if (exceptionCollection.Count > 0)
				throw new AggregateException("One or more exceptions occurred while rolling back.", exceptionCollection);
		}
	}
}
