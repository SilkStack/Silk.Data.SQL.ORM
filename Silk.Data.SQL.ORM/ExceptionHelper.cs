using System;

namespace Silk.Data.SQL.ORM
{
	internal static class ExceptionHelper
	{
		public static void ThrowNotPresentInSchema<T>()
			=> throw new InvalidOperationException($"Entity type `{typeof(T).FullName}` is not present in the schema.");

		public static void ThrowEntityFieldNotFound()
			=> throw new InvalidOperationException("Specified field has no binding to the entity model.");

		public static void ThrowNoPrimaryKey<T>()
			=> throw new InvalidOperationException($"Entity type `{typeof(T).FullName}` has no primary key.");

		public static void ThrowJoinsRequired()
			=> throw new InvalidOperationException("Query requires one or more JOINs, use sub-queries instead.");
	}
}
