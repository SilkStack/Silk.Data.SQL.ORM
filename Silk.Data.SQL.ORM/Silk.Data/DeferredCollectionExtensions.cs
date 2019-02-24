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

		private static DeferredExecutor ToExecutor(this IEnumerable<IDeferred> deferredTasks)
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
