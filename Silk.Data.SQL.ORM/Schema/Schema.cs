using Silk.Data.SQL.ORM.Expressions;
using System;
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
		private readonly Dictionary<Type, EntityModel> _entityModels =
			new Dictionary<Type, EntityModel>();
		private readonly Dictionary<MethodInfo, IMethodCallConverter> _methodCallConverters;

		public Schema(
			IEnumerable<EntityModel> entityModels,
			Dictionary<MethodInfo, IMethodCallConverter> methodCallConverters
			)
		{
			_entityModels = entityModels.ToDictionary(
				q => q.EntityType,
				q => q
				);
			_methodCallConverters = methodCallConverters;
		}

		/// <summary>
		/// Get all entity models for the provided type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public EntityModel<T> GetEntityModel<T>()
			where T : class
		{
			if (_entityModels.TryGetValue(typeof(T), out var entityModel))
				return entityModel as EntityModel<T>;
			return null;
		}

		public IMethodCallConverter GetMethodCallConverter(MethodInfo methodInfo)
		{
			if (methodInfo.IsGenericMethod)
				methodInfo = methodInfo.GetGenericMethodDefinition();
			_methodCallConverters.TryGetValue(methodInfo, out var converter);
			return converter;
		}
	}
}
