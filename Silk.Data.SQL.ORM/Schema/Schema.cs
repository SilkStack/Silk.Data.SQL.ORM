using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.GenericDispatch;
using Silk.Data.SQL.ORM.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

		public PrimaryKeyEntityReferenceFactory<T> CreatePrimaryKeyReferenceFactory<T>()
			where T : class
		{
			var entityModel = GetEntityModel<T>();
			if (entityModel == null)
				ExceptionHelper.ThrowNotPresentInSchema<T>();

			var primaryKeyFields = entityModel.Fields.Where(q => q.IsPrimaryKey).ToArray();
			if (primaryKeyFields.Length != 1)
				throw new InvalidOperationException("Primary key entity references must have exactly 1 primary key field.");

			var entityView = entityModel.GetEntityView(entityModel.TypeModel);
			var primaryKeyIntersectedFields = entityView.EntityToClassIntersection
				.IntersectedFields.FirstOrDefault(q => q.LeftField == primaryKeyFields[0]);
			if (primaryKeyIntersectedFields == null)
				throw new NullReferenceException("Could not determine how entity primary key maps onto entity class type.");

			var builder = new PrimaryKeyFactoryBuilder<T>();
			primaryKeyIntersectedFields.Dispatch(builder);
			return builder.Factory;
		}

		private readonly static Dictionary<Type, Delegate> _parseMethods
			= new Dictionary<Type, Delegate>();

		private static TryConvertDelegate<TFrom, TTo> TryParseFactory<TFrom, TTo>()
		{
			var toType = typeof(TTo);
			if (_parseMethods.TryGetValue(toType, out var @delegate))
				return @delegate as TryConvertDelegate<TFrom, TTo>;

			lock (_parseMethods)
			{
				if (_parseMethods.TryGetValue(toType, out @delegate))
					return @delegate as TryConvertDelegate<TFrom, TTo>;

				var tryParseMethod = GetTryParseMethod(typeof(TTo));
				if (tryParseMethod == null)
				{
					_parseMethods.Add(toType, null);
					return null;
				}

				var fromParameter = Expression.Parameter(typeof(TFrom), "from");
				var toParameter = Expression.Parameter(typeof(TTo).MakeByRefType(), "to");
				var body = Expression.Call(tryParseMethod, fromParameter, toParameter);
				@delegate = Expression.Lambda<TryConvertDelegate<TFrom, TTo>>(body, fromParameter, toParameter).Compile();

				_parseMethods.Add(toType, @delegate);
				return @delegate as TryConvertDelegate<TFrom, TTo>;
			}
		}

		private static MethodInfo GetTryParseMethod(Type toType)
		{
			return toType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(
				q => q.Name == "TryParse" && q.IsStatic && q.GetParameters().Length == 2
			);
		}

		private class PrimaryKeyFactoryBuilder<T> : IIntersectedFieldsGenericExecutor
			where T : class
		{
			public PrimaryKeyEntityReferenceFactory<T> Factory { get; private set; }

			void IIntersectedFieldsGenericExecutor.Execute<TLeftModel, TLeftField, TRightModel, TRightField, TLeftData, TRightData>(
				IntersectedFields<TLeftModel, TLeftField, TRightModel, TRightField, TLeftData, TRightData> intersectedFields
				)
			{
				var parser = TryParseFactory<string, TLeftData>();
				if (parser == null)
					throw new InvalidOperationException($"Can't determine method for parsing strings to type `{typeof(TLeftData).Name}`");
				Factory = new PrimaryKeyEntityReferenceFactory<T, TLeftData>(
					intersectedFields.RightPath as IFieldPath<TypeModel, PropertyInfoField>,
					parser
					);
			}
		}
	}
}
