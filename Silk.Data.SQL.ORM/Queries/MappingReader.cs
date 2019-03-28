using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Queries
{
	public class MappingReader<T> : IResultReader<T>
		where T : class
	{
		private readonly IMapping<EntityModel, EntityField, TypeModel, PropertyInfoField> _mapping;

		public ITypeInstanceFactory TypeInstanceFactory { get; set; }
			= DefaultTypeInstanceFactory.Instance;
		public IReaderWriterFactory<TypeModel, PropertyInfoField> TypeReaderWriterFactory { get; set; }
			= DefaultReaderWriterFactory.Instance;

		public MappingReader(IMapping<EntityModel, EntityField, TypeModel, PropertyInfoField> mapping)
		{
			_mapping = mapping;
		}

		public T Read(QueryResult queryResult)
		{
			var graph = TypeInstanceFactory.CreateInstance<T>();
			var outputWriter = TypeReaderWriterFactory.CreateGraphWriter<T>(graph);
			var inputReader = new QueryGraphReader(queryResult);

			_mapping.Map(inputReader, outputWriter);

			return graph;
		}
	}

	public class DefaultReaderWriterFactory : IReaderWriterFactory<TypeModel, PropertyInfoField>
	{
		public static DefaultReaderWriterFactory Instance { get; }
			= new DefaultReaderWriterFactory();

		public IGraphReader<TypeModel, PropertyInfoField> CreateGraphReader<T>(T graph)
			=> new ObjectGraphReader<T>(graph);

		public IGraphWriter<TypeModel, PropertyInfoField> CreateGraphWriter<T>(T graph)
			where T : class
			=> new ObjectGraphReaderWriter<T>(graph);
	}

	public class DefaultTypeInstanceFactory : ITypeInstanceFactory
	{
		public static DefaultTypeInstanceFactory Instance { get; } =
			new DefaultTypeInstanceFactory();

		public T CreateInstance<T>()
		{
			var factory = GetFactory<T>();
			return factory();
		}

		private static readonly Dictionary<Type, Delegate> _factories
			= new Dictionary<Type, Delegate>();

		private static ConstructorInfo GetParameterlessConstructor(Type type)
		{
			return type
				.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.FirstOrDefault(ctor => ctor.GetParameters().Length == 0);
		}

		private static Func<T> GetFactory<T>()
		{
			var type = typeof(T);
			if (_factories.TryGetValue(type, out var factory))
				return factory as Func<T>;

			lock (_factories)
			{
				if (_factories.TryGetValue(type, out factory))
					return factory as Func<T>;

				factory = CreateFactory<T>();
				_factories.Add(type, factory);
				return factory as Func<T>;
			}
		}

		private static Func<T> CreateFactory<T>()
		{
			var ctor = GetParameterlessConstructor(typeof(T));
			if (ctor == null)
				throw new InvalidOperationException($"{typeof(T).FullName} doesn't have a parameterless constructor.");

			var lambda = Expression.Lambda<Func<T>>(
				Expression.New(ctor)
				);
			return lambda.Compile();
		}
	}
}
