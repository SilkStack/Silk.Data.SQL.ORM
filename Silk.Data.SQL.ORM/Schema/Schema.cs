using Silk.Data.SQL.ORM.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Data schema.
	/// </summary>
	public class Schema
	{
		private readonly List<EntityModel> _entityModels =
			new List<EntityModel>();
		private readonly Dictionary<MethodInfo, IMethodCallConverter> _methodCallConverters;

		public Schema(
			IEnumerable<EntityModel> entityModels,
			Dictionary<MethodInfo, IMethodCallConverter> methodCallConverters
			)
		{
			_entityModels.AddRange(entityModels);
			_methodCallConverters = methodCallConverters;
		}

		/// <summary>
		/// Get all entity models for the provided type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IEnumerable<EntityModel<T>> GetAll<T>()
			where T : class
			=> _entityModels.OfType<EntityModel<T>>();

		public IMethodCallConverter GetMethodCallConverter(MethodInfo methodInfo)
		{
			if (methodInfo.IsGenericMethod)
				methodInfo = methodInfo.GetGenericMethodDefinition();
			_methodCallConverters.TryGetValue(methodInfo, out var converter);
			return converter;
		}
	}
}
