using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Silk.Data
{
	public class DeferredExecutor
	{
		private readonly List<IDeferred> _tasks = new List<IDeferred>();

		public void Add(IDeferred deferred)
		{
			var newBatch = deferred as IDeferredBatch;
			if (newBatch != null)
			{
				var lastBatch = _tasks.LastOrDefault() as IDeferredBatch;
				if (lastBatch != null && lastBatch.TryMerge(newBatch))
					return;
			}
			_tasks.Add(deferred);
		}

		public void Execute()
		{
			foreach (var task in _tasks)
				task.Execute();
		}

		public async Task ExecuteAsync()
		{
			foreach (var task in _tasks)
				await task.ExecuteAsync();
		}
	}
}
