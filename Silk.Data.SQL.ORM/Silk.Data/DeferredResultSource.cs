namespace Silk.Data
{
	public abstract class DeferredResultSource
	{
	}

	public class DeferredResultSource<T> : DeferredResultSource
	{
		public DeferredResult<T> DeferredResult { get; } = new DeferredResult<T>();

		public void SetResult(T result)
		{
			DeferredResult.Result = result;
		}
	}
}
