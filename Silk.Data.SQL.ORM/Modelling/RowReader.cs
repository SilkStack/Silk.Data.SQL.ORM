using Silk.Data.Modelling;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class RowReader
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

		private readonly ViewReadWriter _viewReadWriter;

		public RowReader(ViewReadWriter viewReadWriter)
		{
			_viewReadWriter = viewReadWriter;
		}

		public void ReadRow(QueryResult queryResult)
		{
			ReadViewFields(queryResult, _viewReadWriter.View);
		}

		public void ReadRelatedRow(QueryResult queryResult, IView view, string relatedFieldName)
		{
			var aliasPrefix = "";
			if (relatedFieldName != null)
				aliasPrefix = $"{relatedFieldName}_";

			var objValues = new Dictionary<string, object>();
			foreach (var field in view.Fields
				.OfType<DataField>())
			{
				if (!_typeReaders.TryGetValue(field.DataType, out var readFunc))
					throw new InvalidOperationException("Unsupported data type.");

				var ord = queryResult.GetOrdinal($"{aliasPrefix}{field.Name}");

				if (queryResult.IsDBNull(ord))
					objValues[field.Name] = null;
				else
					objValues[field.Name] = readFunc(queryResult, ord);
			}

			var relatedFieldPath = new[] { relatedFieldName };
			var existingValue = _viewReadWriter.ReadFromPath<List<Dictionary<string,object>>>(relatedFieldPath);
			if (existingValue == null)
				existingValue = new List<Dictionary<string, object>>();
			existingValue.Add(objValues);

			_viewReadWriter.WriteToPath(relatedFieldPath, existingValue);
		}

		private void ReadViewFields(QueryResult queryResult, IView view, string aliasPath = null)
		{
			//  todo: splitting on '_' seems ineffecient, improve that
			var aliasPrefix = "";
			if (aliasPath != null)
				aliasPrefix = $"{aliasPath}_";

			foreach (var field in view.Fields
				.OfType<DataField>())
			{
				if (field.Relationship?.RelationshipType == RelationshipType.ManyToMany)
					continue;

				if (field.Storage == null)
				{
					ReadViewFields(queryResult, field.Relationship.ProjectedModel ?? field.Relationship.ForeignModel, $"{aliasPrefix}{field.Name}");
					continue;
				}

				if (!_typeReaders.TryGetValue(field.DataType, out var readFunc))
					throw new InvalidOperationException("Unsupported data type.");

				var ord = queryResult.GetOrdinal($"{aliasPrefix}{field.Name}");

				if (queryResult.IsDBNull(ord))
					_viewReadWriter.WriteToPath<object>($"{aliasPrefix}{field.Name}".Split('_'), null);
				else
					_viewReadWriter.WriteToPath<object>($"{aliasPrefix}{field.Name}".Split('_'), readFunc(queryResult, ord));
			}
		}
	}
}
