using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Operations.Expressions;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Operations
{
	public class SelectOperation<T> : DataOperationWithResult<ICollection<T>>
	{
		private readonly static string[] _selfPath = new string[] { "." };
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
				{ typeof(bool?), (q,o) => q.GetBoolean(o) },
				{ typeof(byte?), (q,o) => q.GetByte(o) },
				{ typeof(short?), (q,o) => q.GetInt16(o) },
				{ typeof(int?), (q,o) => q.GetInt32(o) },
				{ typeof(long?), (q,o) => q.GetInt64(o) },
				{ typeof(float?), (q,o) => q.GetFloat(o) },
				{ typeof(double?), (q,o) => q.GetDouble(o) },
				{ typeof(decimal?), (q,o) => q.GetDecimal(o) },
				{ typeof(Guid?), (q,o) => q.GetGuid(o) },
				{ typeof(DateTime?), (q,o) => q.GetDateTime(o) },
			};

		private readonly QueryExpression _query;
		private readonly ProjectionModel _projectionModel;
		private readonly TypeModel<T> _typeModel;
		private ICollection<T> _result;

		public override ICollection<T> Result => _result;
		public override bool CanBeBatched => true;

		public SelectOperation(QueryExpression query, ProjectionModel projectionModel)
		{
			_query = query;
			_projectionModel = projectionModel;
			_typeModel = TypeModel.GetModelOf<T>();
		}

		public override QueryExpression GetQuery()
		{
			return _query;
		}

		public override void ProcessResult(QueryResult queryResult)
		{
			if (_projectionModel == null)
			{
				if (!_typeReaders.TryGetValue(typeof(T), out var readerFunc))
					throw new InvalidOperationException("Cannot read data type.");
				var result = new List<T>();
				while (queryResult.Read())
				{
					if (queryResult.IsDBNull(0))
						result.Add(default(T));
					else
						result.Add((T)readerFunc(queryResult, 0));
				}
				queryResult.NextResult();
				_result = result;
			}
			else
			{
				var result = new List<IModelReadWriter>();
				var reader = new QueryResultReader(_projectionModel, queryResult);

				while (queryResult.Read())
				{
					var writer = new ObjectReadWriter(null, _typeModel, typeof(T));
					_projectionModel.Mapping.PerformMapping(reader, writer);
					result.Add(writer);
				}
				queryResult.NextResult();

				foreach (var manyRelatedObjectField in _projectionModel.Fields.OfType<IManyRelatedObjectField>())
				{
					if (!queryResult.HasRows)
						continue;

					reader = new QueryResultReader(manyRelatedObjectField.RelatedObjectModel, queryResult);
					var mapper = manyRelatedObjectField.CreateObjectMapper($"__IDENT__{manyRelatedObjectField.FieldName}");
					mapper.PerformMapping(queryResult, reader, result);
					queryResult.NextResult();
				}

				_result = result.Select(q => q.ReadField<T>(_selfPath, 0)).ToArray();
			}
		}

		public override async Task ProcessResultAsync(QueryResult queryResult)
		{
			if (_projectionModel == null)
			{
				if (!_typeReaders.TryGetValue(typeof(T), out var readerFunc))
					throw new InvalidOperationException("Cannot read data type.");
				var result = new List<T>();
				while (await queryResult.ReadAsync())
				{
					if (queryResult.IsDBNull(0))
						result.Add(default(T));
					else
						result.Add((T)readerFunc(queryResult, 0));
				}
				await queryResult.NextResultAsync();
				_result = result;
			}
			else
			{
				var result = new List<IModelReadWriter>();
				var reader = new QueryResultReader(_projectionModel, queryResult);

				while (await queryResult.ReadAsync())
				{
					var writer = new ObjectReadWriter(null, _typeModel, typeof(T));
					_projectionModel.Mapping.PerformMapping(reader, writer);
					result.Add(writer);
				}
				await queryResult.NextResultAsync();

				foreach (var manyRelatedObjectField in _projectionModel.Fields.OfType<IManyRelatedObjectField>())
				{
					if (!queryResult.HasRows)
						continue;

					reader = new QueryResultReader(manyRelatedObjectField.RelatedObjectModel, queryResult);
					var mapper = manyRelatedObjectField.CreateObjectMapper($"__IDENT__{manyRelatedObjectField.FieldName}");
					await mapper.PerformMappingAsync(queryResult, reader, result);
					await queryResult.NextResultAsync();
				}

				_result = result.Select(q => q.ReadField<T>(_selfPath, 0)).ToArray();
			}
		}
	}

	public class SelectOperation
	{
		public static SelectOperation<TEntity> Create<TEntity>(EntityModel<TEntity> model,
			Condition where = null, Condition having = null,
			OrderBy orderBy = null, GroupBy groupBy = null,
			int? offset = null, int? limit = null)
		{
			return Create<TEntity>(model, model, where, having, orderBy, groupBy, offset, limit);
		}

		public static SelectOperation<TProjection> Create<TEntity, TProjection>(EntityModel<TEntity> model,
			Condition where = null, Condition having = null,
			OrderBy orderBy = null, GroupBy groupBy = null,
			int? offset = null, int? limit = null)
		{
			return Create<TProjection>(model.GetProjection<TProjection>(), model, where, having, orderBy, groupBy, offset, limit);
		}

		public static SelectOperation<int> CreateCount<TEntity>(EntityModel<TEntity> model,
			Condition where = null, Condition having = null, GroupBy groupBy = null)
		{
			var query = CreateQuery(new[] { QueryExpression.CountFunction() }, model,
				where, having, null, groupBy);
			return new SelectOperation<int>(query, null);
		}

		private static SelectOperation<TProjection> Create<TProjection>(ProjectionModel projectionModel, EntityModel entityModel,
			Condition where = null, Condition having = null,
			OrderBy orderBy = null, GroupBy groupBy = null,
			int? offset = null, int? limit = null)
		{
			var query = CreateQuery(projectionModel, entityModel, where, having, orderBy, groupBy, offset, limit);
			return new SelectOperation<TProjection>(query, projectionModel);
		}

		private static QueryExpression CreateQuery(ProjectionModel projectionModel, EntityModel entityModel,
			Condition where = null, Condition having = null,
			OrderBy orderBy = null, GroupBy groupBy = null,
			int? offset = null, int? limit = null)
		{
			var from = QueryExpression.Table(entityModel.EntityTable.TableName);
			var queries = new List<QueryExpression>();
			var projectedFieldsExprs = new List<QueryExpression>();
			var joinExprs = new List<JoinExpression>();
			var whereExpr = where?.GetExpression();
			var havingExpr = having?.GetExpression();
			var offsetExpr = offset == null ? null : QueryExpression.Value(offset.Value);
			var limitExpr = limit == null ? null : QueryExpression.Value(limit.Value);
			var orderByExprs = orderBy?.GetExpressions();
			var groupByExprs = groupBy?.GetExpressions();

			AddJoins(entityModel, projectedFieldsExprs, from, joinExprs);
			AddFields(projectionModel, projectedFieldsExprs, from, joinExprs);

			var query = QueryExpression.Select(
				projectedFieldsExprs.ToArray(),
				from: from,
				joins: joinExprs.Count < 1 ? null : joinExprs.ToArray(),
				where: whereExpr,
				having: havingExpr,
				offset: offsetExpr,
				limit: limitExpr,
				orderBy: orderByExprs,
				groupBy: groupByExprs
				);
			queries.Add(query);

			foreach (var field in projectionModel.Fields.OfType<IManyRelatedObjectField>())
			{
				projectedFieldsExprs.Clear();
				joinExprs.Clear();

				AddJoin(field, from, projectedFieldsExprs, joinExprs, "");
				AddField(field, from, projectedFieldsExprs, joinExprs, "");

				query = QueryExpression.Select(
					projectedFieldsExprs.ToArray(),
					from: from,
					joins: joinExprs.Count < 1 ? null : joinExprs.ToArray(),
					where: whereExpr,
					having: havingExpr
					);
				queries.Add(query);
			}

			if (queries.Count == 1)
				return queries[0];
			return new CompositeQueryExpression(queries.ToArray());
		}

		private static QueryExpression CreateQuery(QueryExpression[] projection, EntityModel entityModel,
			Condition where = null, Condition having = null,
			OrderBy orderBy = null, GroupBy groupBy = null,
			int? offset = null, int? limit = null)
		{
			var from = QueryExpression.Table(entityModel.EntityTable.TableName);
			var queries = new List<QueryExpression>();
			var projectedFieldsExprs = new List<QueryExpression>();
			var joinExprs = new List<JoinExpression>();
			var whereExpr = where?.GetExpression();
			var havingExpr = having?.GetExpression();
			var offsetExpr = offset == null ? null : QueryExpression.Value(offset.Value);
			var limitExpr = limit == null ? null : QueryExpression.Value(limit.Value);
			var orderByExprs = orderBy?.GetExpressions();
			var groupByExprs = groupBy?.GetExpressions();

			AddJoins(entityModel, projectedFieldsExprs, from, joinExprs);

			return QueryExpression.Select(
				projection,
				from: from,
				joins: joinExprs.Count < 1 ? null : joinExprs.ToArray(),
				where: whereExpr,
				having: havingExpr,
				offset: offsetExpr,
				limit: limitExpr,
				orderBy: orderByExprs,
				groupBy: groupByExprs
				);
		}

		private static void AddFields(ProjectionModel model, List<QueryExpression> projectedFieldsExprs, QueryExpression from, List<JoinExpression> joins,
			string fieldPrefix = "")
		{
			foreach (var field in model.Fields)
			{
				if (field is IManyRelatedObjectField)
					continue;
				AddField(field, from, projectedFieldsExprs, joins, fieldPrefix);
			}
		}

		private static void AddField(IEntityField field, QueryExpression from, List<QueryExpression> projectedFieldsExprs, List<JoinExpression> joins, string fieldPrefix)
		{
			var fromSource = from;
			if (fromSource is AliasExpression aliasExpression)
				fromSource = aliasExpression.Identifier;

			if (field is IValueField valueField)
			{
				projectedFieldsExprs.Add(
					QueryExpression.Alias(
						QueryExpression.Column(valueField.Column.ColumnName, fromSource),
						$"{fieldPrefix}{valueField.FieldName}"
					));
			}
			else if (field is IEmbeddedObjectField embeddedObjectField)
			{
				projectedFieldsExprs.Add(
					QueryExpression.Alias(
						QueryExpression.Column(embeddedObjectField.NullCheckColumn.ColumnName, fromSource),
						$"{fieldPrefix}{embeddedObjectField.FieldName}"
					));

				foreach (var subField in embeddedObjectField.EmbeddedFields)
					AddField(subField, from, projectedFieldsExprs, joins, $"{fieldPrefix}{field.FieldName}_");
			}
			else if (field is ISingleRelatedObjectField singleRelationshipField)
			{
				var joinAlias = QueryExpression.Alias(
					QueryExpression.Table(singleRelationshipField.RelatedObjectModel.EntityTable.TableName),
					$"{fieldPrefix}{singleRelationshipField.FieldName}"
					);

				AddFields(singleRelationshipField.RelatedObjectProjection, projectedFieldsExprs, joinAlias, joins, $"{fieldPrefix}{field.FieldName}_");
			}
			else if (field is IManyRelatedObjectField manyRelationshipField)
			{
				var junctionJoinAlias = QueryExpression.Alias(
					QueryExpression.Table(manyRelationshipField.JunctionTable.TableName),
					$"__JUNCTION__{fieldPrefix}{manyRelationshipField.FieldName}"
					);

				var objectJoinAlias = QueryExpression.Alias(
					QueryExpression.Table(manyRelationshipField.RelatedObjectModel.EntityTable.TableName),
					$"{fieldPrefix}{manyRelationshipField.FieldName}"
					);

				AddFields(manyRelationshipField.RelatedObjectProjection, projectedFieldsExprs, objectJoinAlias, joins, "");
			}
		}

		private static void AddJoins(ProjectionModel model, List<QueryExpression> projectedFieldsExprs, QueryExpression from, List<JoinExpression> joins,
			string fieldPrefix = "")
		{
			foreach (var field in model.Fields)
			{
				if (field is IManyRelatedObjectField)
					continue;
				AddJoin(field, from, projectedFieldsExprs, joins, fieldPrefix);
			}
		}

		private static void AddJoin(IEntityField field, QueryExpression from, List<QueryExpression> projectedFieldsExprs, List<JoinExpression> joins, string fieldPrefix)
		{
			var fromSource = from;
			if (fromSource is AliasExpression aliasExpression)
				fromSource = aliasExpression.Identifier;

			if (field is IEmbeddedObjectField embeddedObjectField)
			{
				foreach (var subField in embeddedObjectField.EmbeddedFields)
					AddJoin(subField, from, projectedFieldsExprs, joins, $"{fieldPrefix}{field.FieldName}_");
			}
			if (field is ISingleRelatedObjectField singleRelationshipField)
			{
				projectedFieldsExprs.Add(
					QueryExpression.Alias(
						QueryExpression.Column(singleRelationshipField.LocalColumn.ColumnName, fromSource),
						$"{fieldPrefix}{singleRelationshipField.FieldName}"
					));

				var joinAlias = QueryExpression.Alias(
					QueryExpression.Table(singleRelationshipField.RelatedObjectModel.EntityTable.TableName),
					$"{fieldPrefix}{singleRelationshipField.FieldName}"
					);
				var joinExpr = QueryExpression.Join(
					QueryExpression.Column(singleRelationshipField.LocalColumn.ColumnName, from),
					QueryExpression.Column(singleRelationshipField.RelatedPrimaryKey.Column.ColumnName, joinAlias),
					JoinDirection.Left
					);

				joins.Add(joinExpr);
				AddJoins(singleRelationshipField.RelatedObjectModel, projectedFieldsExprs, joinAlias, joins, $"{fieldPrefix}{field.FieldName}_");
			}
			else if (field is IManyRelatedObjectField manyRelationshipField)
			{
				projectedFieldsExprs.Add(
					QueryExpression.Alias(
						QueryExpression.Column(manyRelationshipField.LocalColumn.ColumnName, fromSource),
						$"__IDENT__{fieldPrefix}{manyRelationshipField.FieldName}"
					));

				var junctionJoinAlias = QueryExpression.Alias(
					QueryExpression.Table(manyRelationshipField.JunctionTable.TableName),
					$"__JUNCTION__{fieldPrefix}{manyRelationshipField.FieldName}"
					);
				var junctionJoin = QueryExpression.Join(
					QueryExpression.Column(manyRelationshipField.LocalColumn.ColumnName, from),
					QueryExpression.Column(manyRelationshipField.LocalJunctionColumn.ColumnName, junctionJoinAlias),
					JoinDirection.Inner
					);
				joins.Add(junctionJoin);

				var objectJoinAlias = QueryExpression.Alias(
					QueryExpression.Table(manyRelationshipField.RelatedObjectModel.EntityTable.TableName),
					$"{fieldPrefix}{manyRelationshipField.FieldName}"
					);
				var objectJoin = QueryExpression.Join(
					QueryExpression.Column(manyRelationshipField.RelatedJunctionColumn.ColumnName, junctionJoinAlias),
					QueryExpression.Column(manyRelationshipField.RelatedPrimaryKey.Column.ColumnName, objectJoinAlias),
					JoinDirection.Inner
					);
				joins.Add(objectJoin);

				AddJoins(manyRelationshipField.RelatedObjectModel, projectedFieldsExprs, objectJoinAlias, joins, "");
			}
		}
	}
}
