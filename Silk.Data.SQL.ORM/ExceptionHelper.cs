using System;

namespace Silk.Data.SQL.ORM
{
	internal static class ExceptionHelper
	{
		public static void ThrowNotPresentInSchema<T>()
			=> throw new InvalidOperationException($"Entity type `{typeof(T).FullName}` is not present in the schema.");
	}
}
