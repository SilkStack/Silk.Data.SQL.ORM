﻿using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	/// <summary>
	/// Takes the query result from a built query and maps it to object instances.
	/// </summary>
	public abstract class ResultMapper
	{
		public int ResultSetCount { get; }

		public ResultMapper(int resultSetCount)
		{
			ResultSetCount = resultSetCount;
		}
	}

	public class ValueResultMapper<T> : ResultMapper
	{
		private readonly IFieldReference _fieldReference;

		public ValueResultMapper(int resultSetCount, IFieldReference fieldReference)
			: base(resultSetCount)
		{
			_fieldReference = fieldReference;
		}

		protected virtual TSource ReadField<TSource>(QueryResultReader queryResultReader)
		{
			return queryResultReader.ReadField<TSource>(_fieldReference);
		}

		public T Read(QueryResult queryResult)
		{
			var queryReader = new QueryResultReader(queryResult);
			return ReadField<T>(queryReader);
		}

		public ICollection<T> ReadResultSet(QueryResult queryResult)
		{
			var result = new List<T>();
			var queryReader = new QueryResultReader(queryResult);
			while (queryResult.Read())
			{
				result.Add(ReadField<T>(queryReader));
			}
			return result;
		}

		public async Task<ICollection<T>> ReadResultSetAsync(QueryResult queryResult)
		{
			var result = new List<T>();
			var queryReader = new QueryResultReader(queryResult);
			while (await queryResult.ReadAsync())
			{
				result.Add(ReadField<T>(queryReader));
			}
			return result;
		}

		public ICollection<T> ReadAll(QueryResult queryResult)
		{
			var result = new List<T>();
			var queryReader = new QueryResultReader(queryResult);
			for (var i = 0; i < ResultSetCount; i++)
			{
				while (queryResult.Read())
				{
					result.Add(ReadField<T>(queryReader));
				}

				if (!queryResult.NextResult())
					break;
			}
			return result;
		}

		public async Task<ICollection<T>> ReadAllAsync(QueryResult queryResult)
		{
			var result = new List<T>();
			var queryReader = new QueryResultReader(queryResult);
			for (var i = 0; i < ResultSetCount; i++)
			{
				while (await queryResult.ReadAsync())
				{
					result.Add(ReadField<T>(queryReader));
				}

				if (!await queryResult.NextResultAsync())
					break;
			}
			return result;
		}
	}

	public class TransformedValueResultMapper<TResult, TSource> : ValueResultMapper<TResult>
	{
		private readonly Func<TSource, TResult> _transform;

		public TransformedValueResultMapper(int resultSetCount, IFieldReference fieldReference,
			Func<TSource, TResult> transform)
			: base(resultSetCount, fieldReference)
		{
			_transform = transform;
		}

		protected override T ReadField<T>(QueryResultReader queryResultReader)
		{
			var value = base.ReadField<TSource>(queryResultReader);
			var transform = _transform as Func<TSource, T>;
			return transform(value);
		}
	}

	public class ObjectResultMapper<T> : ResultMapper
	{
		private readonly static TypeModel<T> _typeModel = TypeModel.GetModelOf<T>();

		private readonly Mapping _mapping;

		public ObjectResultMapper(int resultSetCount, Mapping mapping)
			: base(resultSetCount)
		{
			_mapping = mapping;
		}

		public void Inject(T obj, QueryResult queryResult)
		{
			var objectReadWriter = new ObjectReadWriter(obj, _typeModel, typeof(T));
			var queryReader = new QueryResultReader(queryResult);
			_mapping.PerformMapping(queryReader, objectReadWriter);
		}

		public T Map(QueryResult queryResult)
		{
			var objectReadWriter = new ObjectReadWriter(null, _typeModel, typeof(T));
			var queryReader = new QueryResultReader(queryResult);
			_mapping.PerformMapping(queryReader, objectReadWriter);
			return objectReadWriter.ReadField<T>(_typeModel.Root);
		}

		public ICollection<T> MapResultSet(QueryResult queryResult)
		{
			var queryReader = new QueryResultReader(queryResult);
			var result = new List<T>();
			while (queryResult.Read())
			{
				var objectReadWriter = new ObjectReadWriter(null, _typeModel, typeof(T));
				_mapping.PerformMapping(queryReader, objectReadWriter);
				result.Add(objectReadWriter.ReadField<T>(_typeModel.Root));
			}
			return result;
		}

		public async Task<ICollection<T>> MapResultSetAsync(QueryResult queryResult)
		{
			var queryReader = new QueryResultReader(queryResult);
			var result = new List<T>();
			while (await queryResult.ReadAsync())
			{
				var objectReadWriter = new ObjectReadWriter(null, _typeModel, typeof(T));
				_mapping.PerformMapping(queryReader, objectReadWriter);
				result.Add(objectReadWriter.ReadField<T>(_typeModel.Root));
			}
			return result;
		}

		public ICollection<T> MapAll(QueryResult queryResult)
		{
			var queryReader = new QueryResultReader(queryResult);
			var result = new List<T>();
			for (var i = 0; i < ResultSetCount; i++)
			{
				while (queryResult.Read())
				{
					var objectReadWriter = new ObjectReadWriter(null, _typeModel, typeof(T));
					_mapping.PerformMapping(queryReader, objectReadWriter);
					result.Add(objectReadWriter.ReadField<T>(_typeModel.Root));
				}

				if (!queryResult.NextResult())
					break;
			}
			return result;
		}

		public async Task<ICollection<T>> MapAllAsync(QueryResult queryResult)
		{
			var queryReader = new QueryResultReader(queryResult);
			var result = new List<T>();
			for (var i = 0; i < ResultSetCount; i++)
			{
				while (await queryResult.ReadAsync())
				{
					var objectReadWriter = new ObjectReadWriter(null, _typeModel, typeof(T));
					_mapping.PerformMapping(queryReader, objectReadWriter);
					result.Add(objectReadWriter.ReadField<T>(_typeModel.Root));
				}

				if (!await queryResult.NextResultAsync())
					break;
			}
			return result;
		}
	}

	public class TupleResultMapper<T1, T2> : ResultMapper
	{
		private readonly static TypeModel<T1> _type1Model = TypeModel.GetModelOf<T1>();
		private readonly static TypeModel<T2> _type2Model = TypeModel.GetModelOf<T2>();

		private readonly Binding[] _t1Bindings;
		private readonly Binding[] _t2Bindings;

		public TupleResultMapper(int resultSetCount, IEnumerable<Binding> t1Bindings,
			IEnumerable<Binding> t2Bindings)
			: base(resultSetCount)
		{
			_t1Bindings = t1Bindings.ToArray();
			_t2Bindings = t2Bindings.ToArray();
		}

		public (T1,T2) Map(QueryResult queryResult)
		{
			var object1ReadWriter = new ObjectReadWriter(null, _type1Model, typeof(T1));
			var object2ReadWriter = new ObjectReadWriter(null, _type2Model, typeof(T2));
			var queryReader = new QueryResultReader(queryResult);
			BindType1(queryReader, object1ReadWriter);
			BindType2(queryReader, object2ReadWriter);
			return (
				object1ReadWriter.ReadField<T1>(_type1Model.Root),
				object2ReadWriter.ReadField<T2>(_type2Model.Root)
				);
		}

		public ICollection<(T1,T2)> MapResultSet(QueryResult queryResult)
		{
			var queryReader = new QueryResultReader(queryResult);
			var result = new List<(T1, T2)>();
			while (queryResult.Read())
			{
				var object1ReadWriter = new ObjectReadWriter(null, _type1Model, typeof(T1));
				var object2ReadWriter = new ObjectReadWriter(null, _type2Model, typeof(T2));
				BindType1(queryReader, object1ReadWriter);
				BindType2(queryReader, object2ReadWriter);
				result.Add((
					object1ReadWriter.ReadField<T1>(_type1Model.Root),
					object2ReadWriter.ReadField<T2>(_type2Model.Root)
				));
			}
			return result;
		}

		public async Task<ICollection<(T1, T2)>> MapResultSetAsync(QueryResult queryResult)
		{
			var queryReader = new QueryResultReader(queryResult);
			var result = new List<(T1, T2)>();
			while (await queryResult.ReadAsync())
			{
				var object1ReadWriter = new ObjectReadWriter(null, _type1Model, typeof(T1));
				var object2ReadWriter = new ObjectReadWriter(null, _type2Model, typeof(T2));
				BindType1(queryReader, object1ReadWriter);
				BindType2(queryReader, object2ReadWriter);
				result.Add((
					object1ReadWriter.ReadField<T1>(_type1Model.Root),
					object2ReadWriter.ReadField<T2>(_type2Model.Root)
				));
			}
			return result;
		}

		public ICollection<(T1, T2)> MapAll(QueryResult queryResult)
		{
			var queryReader = new QueryResultReader(queryResult);
			var result = new List<(T1, T2)>();
			for (var i = 0; i < ResultSetCount; i++)
			{
				while (queryResult.Read())
				{
					var object1ReadWriter = new ObjectReadWriter(null, _type1Model, typeof(T1));
					var object2ReadWriter = new ObjectReadWriter(null, _type2Model, typeof(T2));
					BindType1(queryReader, object1ReadWriter);
					BindType2(queryReader, object2ReadWriter);
					result.Add((
						object1ReadWriter.ReadField<T1>(_type1Model.Root),
						object2ReadWriter.ReadField<T2>(_type2Model.Root)
					));
				}

				if (!queryResult.NextResult())
					break;
			}
			return result;
		}

		public async Task<ICollection<(T1, T2)>> MapAllAsync(QueryResult queryResult)
		{
			var queryReader = new QueryResultReader(queryResult);
			var result = new List<(T1, T2)>();
			for (var i = 0; i < ResultSetCount; i++)
			{
				while (await queryResult.ReadAsync())
				{
					var object1ReadWriter = new ObjectReadWriter(null, _type1Model, typeof(T1));
					var object2ReadWriter = new ObjectReadWriter(null, _type2Model, typeof(T2));
					BindType1(queryReader, object1ReadWriter);
					BindType2(queryReader, object2ReadWriter);
					result.Add((
						object1ReadWriter.ReadField<T1>(_type1Model.Root),
						object2ReadWriter.ReadField<T2>(_type2Model.Root)
					));
				}

				if (!await queryResult.NextResultAsync())
					break;
			}
			return result;
		}

		private void BindType1(IModelReadWriter from, IModelReadWriter to)
		{
			foreach (var binding in _t1Bindings)
			{
				binding.PerformBinding(from, to);
			}
		}

		private void BindType2(IModelReadWriter from, IModelReadWriter to)
		{
			foreach (var binding in _t2Bindings)
			{
				binding.PerformBinding(from, to);
			}
		}
	}
}
