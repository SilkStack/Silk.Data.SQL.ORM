namespace Silk.Data
{
	public abstract class DeferredResult
	{
		public bool TaskHasRun { get; internal set; }
	}

	public class DeferredResult<T> : DeferredResult
	{
		public T Result { get; internal set; }
	}
}
