using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public abstract class ORMQuery
	{
		public Type MapToType { get; }
		public bool IsQueryResult { get; }
		public QueryExpression Query { get; }

		public ORMQuery(QueryExpression query, Type mapToType = null,
			bool isQueryResult = false)
		{
			Query = query;
			MapToType = mapToType;
			IsQueryResult = isQueryResult;
		}

		public virtual object MapResult(QueryResult queryResult)
		{
			return null;
		}

		public virtual Task<object> MapResultAsync(QueryResult queryResult)
		{
			return null;
		}
	}

	public class NoResultORMQuery : ORMQuery
	{
		public NoResultORMQuery(QueryExpression query) : base(query)
		{
		}
	}

	public class AssignAutoIncrementORMQuery : ORMQuery
	{
		private readonly DataField _field;
		private readonly IContainerReadWriter _containerReadWriter;

		public AssignAutoIncrementORMQuery(QueryExpression query, Type mapToType,
			DataField field, IContainerReadWriter containerReadWriter)
			: base(query, mapToType)
		{
			_field = field;
			_containerReadWriter = containerReadWriter;
		}

		public override object MapResult(QueryResult queryResult)
		{
			if (queryResult.Read())
			{
				if (MapToType == typeof(short))
					_field.ModelBinding.WriteValue(_containerReadWriter, queryResult.GetInt16(0));
				else if (MapToType == typeof(long))
					_field.ModelBinding.WriteValue(_containerReadWriter, queryResult.GetInt64(0));
				else if (MapToType == typeof(int))
					_field.ModelBinding.WriteValue(_containerReadWriter, queryResult.GetInt32(0));
			}
			else
				throw new Exception("Failed to get auto generated ID.");
			return null;
		}

		public override async Task<object> MapResultAsync(QueryResult queryResult)
		{
			if (await queryResult.ReadAsync().ConfigureAwait(false))
			{
				if (MapToType == typeof(short))
					_field.ModelBinding.WriteValue(_containerReadWriter, queryResult.GetInt16(0));
				else if (MapToType == typeof(long))
					_field.ModelBinding.WriteValue(_containerReadWriter, queryResult.GetInt64(0));
				else if (MapToType == typeof(int))
					_field.ModelBinding.WriteValue(_containerReadWriter, queryResult.GetInt32(0));
			}
			else
				throw new Exception("Failed to get auto generated ID.");
			return null;
		}
	}

	public abstract class MapResultORMQuery : ORMQuery
	{
		public MapResultORMQuery(QueryExpression query, Type mapToType)
			: base(query, mapToType, true)
		{
		}
	}

	public class ScalarResultORMQuery<T> : MapResultORMQuery
	{
		private static readonly Dictionary<Type, Func<QueryResult, int, object>> _typeReaders =
			new Dictionary<Type, Func<QueryResult, int, object>>()
			{
				{ typeof(bool), (q,o) => q.GetBoolean(o) },
				{ typeof(byte), (q,o) => q.GetByte(o) },
				{ typeof(short), (q,o) => q.GetInt16(o) },
				{ typeof(int), (q,o) => q.GetInt32(o) },
				{ typeof(long), (q,o) => q.GetInt64(o) },
				{ typeof(float), (q,o) => q.GetFloat(o) },
				{ typeof(double), (q,o) => q.GetDouble(o) },
				{ typeof(decimal), (q,o) => q.GetDecimal(o) },
				{ typeof(string), (q,o) => q.GetString(o) },
				{ typeof(Guid), (q,o) => q.GetGuid(o) },
				{ typeof(DateTime), (q,o) => q.GetDateTime(o) },
			};

		private Func<QueryResult, int, object> _typeReader;

		public ScalarResultORMQuery(QueryExpression query)
			: base(query, typeof(T))
		{
			if (!_typeReaders.TryGetValue(typeof(T), out _typeReader))
				throw new InvalidOperationException("Type not supported.");
		}

		public override object MapResult(QueryResult queryResult)
		{
			if (!queryResult.HasRows)
				return new T[0];

			var ret = new List<T>();
			while (queryResult.Read())
			{
				ret.Add((T)_typeReader(queryResult, 0));
			}
			return ret;
		}

		public override async Task<object> MapResultAsync(QueryResult queryResult)
		{
			if (!queryResult.HasRows)
				return new T[0];

			var ret = new List<T>();
			while (await queryResult.ReadAsync()
				.ConfigureAwait(false))
			{
				ret.Add((T)_typeReader(queryResult, 0));
			}
			return ret;
		}
	}

	public class MapResultORMQuery<TView> : MapResultORMQuery
		where TView : new()
	{
		private static TView[] _noResults = new TView[0];

		public EntityModel EntityModel { get; }

		public MapResultORMQuery(QueryExpression query, EntityModel entityModel)
			: base(query, typeof(TView))
		{
			EntityModel = entityModel;
		}

		public override object MapResult(QueryResult queryResult)
		{
			if (!queryResult.HasRows)
				return _noResults;

			var resultWriters = new List<ModelReadWriter>();
			var rowReaders = new Dictionary<string, ViewReadWriter>();
			var resultList = new List<TView>();

			while (queryResult.Read())
			{
				var compositeKey = ReadCompositePrimaryKey(queryResult);
				if (compositeKey == "")
					compositeKey = resultWriters.Count.ToString();
				var result = new TView();
				resultList.Add(result);
				var container = new MemoryViewReadWriter(EntityModel);
				var rowReader = new RowReader(container);
				rowReader.ReadRow(queryResult);

				rowReaders.Add(compositeKey, container);
				resultWriters.Add(new ObjectModelReadWriter(EntityModel.Model, result));
			}

			var manyToManyFields = EntityModel.Fields
				.Where(q => q.Storage == null && q.Relationship != null && q.Relationship.RelationshipType == RelationshipType.ManyToMany)
				.ToArray();
			if (manyToManyFields.Length > 0)
			{
				foreach (var field in manyToManyFields)
				{
					queryResult.NextResult();

					while (queryResult.Read())
					{
						var compositeKey = ReadCompositePrimaryKey(queryResult);
						var rowReader = new RowReader(rowReaders[compositeKey]);
						rowReader.ReadRelatedRow(queryResult,
							field.Relationship.ProjectedModel ?? field.Relationship.ForeignModel,
							field.Name);
					}
				}
			}

			EntityModel.MapToModelAsync(resultWriters, rowReaders.Values)
					.ConfigureAwait(false)
					.GetAwaiter().GetResult();

			return resultList;
		}

		public override async Task<object> MapResultAsync(QueryResult queryResult)
		{
			if (!queryResult.HasRows)
				return _noResults;

			var resultWriters = new List<ModelReadWriter>();
			var rowReaders = new Dictionary<string, ViewReadWriter>();
			var resultList = new List<TView>();

			while (await queryResult.ReadAsync()
				.ConfigureAwait(false))
			{
				var compositeKey = ReadCompositePrimaryKey(queryResult);
				if (compositeKey == "")
					compositeKey = resultWriters.Count.ToString();
				var result = new TView();
				resultList.Add(result);
				var container = new MemoryViewReadWriter(EntityModel);
				var rowReader = new RowReader(container);
				rowReader.ReadRow(queryResult);

				rowReaders.Add(compositeKey, container);
				resultWriters.Add(new ObjectModelReadWriter(EntityModel.Model, result));
			}

			var manyToManyFields = EntityModel.Fields
				.Where(q => q.Storage == null && q.Relationship != null && q.Relationship.RelationshipType == RelationshipType.ManyToMany)
				.ToArray();
			if (manyToManyFields.Length > 0)
			{
				foreach (var field in manyToManyFields)
				{
					await queryResult.NextResultAsync()
						.ConfigureAwait(false);

					while (await queryResult.ReadAsync()
						.ConfigureAwait(false))
					{
						var compositeKey = ReadCompositePrimaryKey(queryResult);
						var rowReader = new RowReader(rowReaders[compositeKey]);
						rowReader.ReadRelatedRow(queryResult,
							field.Relationship.ProjectedModel ?? field.Relationship.ForeignModel,
							field.Name);
					}
				}
			}

			await EntityModel.MapToModelAsync(resultWriters, rowReaders.Values)
					.ConfigureAwait(false);

			return resultList;
		}

		private StringBuilder _compositeKeyBuilder = new StringBuilder();
		private string ReadCompositePrimaryKey(QueryResult queryResult)
		{
			_compositeKeyBuilder.Clear();
			foreach (var field in EntityModel.PrimaryKeyFields)
			{
				if (field.DataType == typeof(Guid))
					_compositeKeyBuilder.Append(queryResult.GetGuid(queryResult.GetOrdinal(field.Name)));
				else
					_compositeKeyBuilder.Append(queryResult.GetInt32(queryResult.GetOrdinal(field.Name)));
				_compositeKeyBuilder.Append(":");
			}
			return _compositeKeyBuilder.ToString();
		}
	}
}
