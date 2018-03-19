using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Operations
{
	public abstract class MultipleObjectMapper
	{
		protected static readonly string[] SelfPath = new string[] { "." };

		protected static readonly Dictionary<Type, Func<QueryResult, int, object>> TypeReaders =
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

		public abstract void PerformMapping(QueryResult queryResult, IModelReadWriter reader, IReadOnlyCollection<IModelReadWriter> results);
		public abstract Task PerformMappingAsync(QueryResult queryResult, IModelReadWriter reader, IReadOnlyCollection<IModelReadWriter> results);
	}

	public class MultipleObjectMapper<T, TElement, TIdentifier> : MultipleObjectMapper
		where T : class, IEnumerable<TElement>
	{
		private readonly string _identityFieldName;
		private readonly IManyRelatedObjectField _field;
		private readonly Func<QueryResult, int, object> _typeReader;

		public MultipleObjectMapper(string identityFieldName, IManyRelatedObjectField field)
		{
			_identityFieldName = identityFieldName;
			_field = field;
			_typeReader = TypeReaders[typeof(TIdentifier)];
		}

		public override void PerformMapping(QueryResult queryResult, IModelReadWriter reader, IReadOnlyCollection<IModelReadWriter> results)
		{
			var objectsToMap = new Dictionary<TIdentifier, List<TElement>>();

			while (queryResult.Read())
			{
				var identifier = ReadIdentifier(queryResult);
				if (!objectsToMap.TryGetValue(identifier, out var list))
				{
					list = new List<TElement>();
					objectsToMap.Add(identifier, list);
				}

				var writer = new ObjectReadWriter(null, _field.ElementModel, typeof(TElement));
				_field.Mapping.PerformMapping(reader, writer);
				list.Add(writer.ReadField<TElement>(SelfPath, 0));
			}

			var identifierPath = new string[] { _field.LocalIdentifierField.FieldName };
			var valuePath = new string[] { _field.FieldName };
			foreach (var result in results)
			{
				var identifier = result.ReadField<TIdentifier>(identifierPath, 0);
				if (objectsToMap.TryGetValue(identifier, out var valueList))
				{
					if (_field.FieldType.IsArray)
						result.WriteField<TElement[]>(valuePath, 0, valueList.ToArray());
					else
						result.WriteField<T>(valuePath, 0, valueList as T);
				}
				else
				{
					if (_field.FieldType.IsArray)
						result.WriteField<TElement[]>(valuePath, 0, new TElement[0]);
					else
						result.WriteField<T>(valuePath, 0, new List<TElement>() as T);
				}
			}
		}

		public override async Task PerformMappingAsync(QueryResult queryResult, IModelReadWriter reader, IReadOnlyCollection<IModelReadWriter> results)
		{
			var objectsToMap = new Dictionary<TIdentifier, List<TElement>>();

			while (await queryResult.ReadAsync())
			{
				var identifier = ReadIdentifier(queryResult);
				if (!objectsToMap.TryGetValue(identifier, out var list))
				{
					list = new List<TElement>();
					objectsToMap.Add(identifier, list);
				}

				var writer = new ObjectReadWriter(null, _field.ElementModel, typeof(TElement));
				_field.Mapping.PerformMapping(reader, writer);
				list.Add(writer.ReadField<TElement>(SelfPath, 0));
			}

			var identifierPath = new string[] { _field.LocalIdentifierField.FieldName };
			var valuePath = new string[] { _field.FieldName };
			foreach (var result in results)
			{
				var identifier = result.ReadField<TIdentifier>(identifierPath, 0);
				if (objectsToMap.TryGetValue(identifier, out var valueList))
				{
					if (_field.FieldType.IsArray)
						result.WriteField<TElement[]>(valuePath, 0, valueList.ToArray());
					else
						result.WriteField<T>(valuePath, 0, valueList as T);
				}
				else
				{
					if (_field.FieldType.IsArray)
						result.WriteField<TElement[]>(valuePath, 0, new TElement[0]);
					else
						result.WriteField<T>(valuePath, 0, new List<TElement>() as T);
				}
			}
		}

		private TIdentifier ReadIdentifier(QueryResult queryResult)
		{
			var ord = queryResult.GetOrdinal(_identityFieldName);
			return (TIdentifier)_typeReader(queryResult, ord);
		}
	}
}
