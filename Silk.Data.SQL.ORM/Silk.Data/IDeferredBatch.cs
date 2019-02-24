namespace Silk.Data
{
	public interface IDeferredBatch : IDeferred
	{
		/// <summary>
		/// Try to merge a new batch into the current batch.
		/// </summary>
		/// <param name="batch"></param>
		/// <returns></returns>
		bool TryMerge(IDeferredBatch batch);
	}
}
