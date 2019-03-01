using System.Threading.Tasks;

namespace Silk.Data
{
	public interface IDeferable
	{
		IDeferred Defer();
		void Execute();
		Task ExecuteAsync();
	}

	public interface IDeferable<T>
	{
		IDeferred Defer(out DeferredResult<T> deferredResult);
		T Execute();
		Task<T> ExecuteAsync();
	}
}
