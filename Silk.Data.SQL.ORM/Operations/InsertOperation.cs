using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Operations.Expressions;
using Silk.Data.SQL.Queries;

namespace Silk.Data.SQL.ORM.Operations
{
	public class InsertOperation : DataOperation
	{
		public bool GeneratesValuesServerSide { get; private set; }

		private readonly QueryExpression _bulkInsertExpression;
		private readonly List<IndividualInsert> _individualInserts;
		private readonly List<QueryExpression> _additionalQueries;

		public override bool CanBeBatched => !GeneratesValuesServerSide;

		private InsertOperation(QueryExpression bulkInsertExpression,
			List<IndividualInsert> individualInserts,
			List<QueryExpression> additionalQueries)
		{
			GeneratesValuesServerSide = individualInserts.Count > 0;
			_bulkInsertExpression = bulkInsertExpression;
			_individualInserts = individualInserts;
			_additionalQueries = additionalQueries;
		}

		public override QueryExpression GetQuery()
		{
			if (_individualInserts == null || _individualInserts.Count < 1)
				return new CompositeQueryExpression(
					new[] { _bulkInsertExpression }.Concat(_additionalQueries).ToArray()
					);

			if (_bulkInsertExpression != null)
				return new CompositeQueryExpression(
					_individualInserts.Select(q => q.Query)
						.Concat(new[] { _bulkInsertExpression })
						.Concat(_additionalQueries)
						.ToArray()
					);
			return new CompositeQueryExpression(
				_individualInserts.Select(q => q.Query).Concat(_additionalQueries).ToArray()
				);
		}

		public override void ProcessResult(QueryResult queryResult)
		{
			if (_individualInserts == null || _individualInserts.Count < 1)
				return;

			foreach (var individualInsert in _individualInserts)
			{
				queryResult.Read();
				individualInsert.MapOntoObject(queryResult);
				queryResult.NextResult();
			}
		}

		public override async Task ProcessResultAsync(QueryResult queryResult)
		{
			if (_individualInserts == null || _individualInserts.Count < 1)
				return;

			foreach (var individualInsert in _individualInserts)
			{
				await queryResult.ReadAsync();
				individualInsert.MapOntoObject(queryResult);
				await queryResult.NextResultAsync();
			}
		}

		public static InsertOperation Create<TEntity>(EntityModel<TEntity> model, params TEntity[] entities)
			where TEntity : class
		{
			return Create<TEntity>(model, model, entities);
		}

		public static InsertOperation Create<TEntity, TProjection>(EntityModel<TEntity> model, params TProjection[] projections)
			where TProjection : class
		{
			return Create<TProjection>(model.GetProjection<TProjection>(), model, projections);
		}

		private static InsertOperation Create<TProjection>(ProjectionModel projectionModel, EntityModel entityModel, params TProjection[] projections)
			where TProjection : class
		{
			var typeModel = TypeModel.GetModelOf<TProjection>();
			var individualInsertExpressions = new List<IndividualInsert>();

			var primaryKeyField = entityModel.Fields.OfType<IValueField>().FirstOrDefault(q => q.Column.IsPrimaryKey);
			var primaryKeyIsOnProjection = false;
			var primaryKeyIsServerGenerated = primaryKeyField?.Column.IsServerGenerated ?? false;
			ServerGeneratedValue serverGeneratedPrimaryKey = null;
			if (primaryKeyField != null)
			{
				primaryKeyIsOnProjection = projectionModel.Fields.OfType<IValueField>().Any(q => q.Column.ColumnName == primaryKeyField.Column.ColumnName);
			}

			var transformer = new ColumnHelperTransformer(projectionModel);
			var columns = new List<ColumnHelper>();
			foreach (var projectionField in projectionModel.Fields)
			{
				if (projectionField is IManyRelatedObjectField)
					continue;
				projectionField.Transform(transformer);
			}
			columns.AddRange(transformer.Current.Where(q => q != null));
			transformer.Current.Clear();
			if (!primaryKeyIsOnProjection && primaryKeyField != null)
			{
				primaryKeyField.Transform(transformer);
				columns.AddRange(transformer.Current.Where(q => q != null));
			}
			var primaryKeyIndex = -1;
			if (primaryKeyField != null)
			{
				primaryKeyIndex = columns.IndexOf(columns.First(q => q.ColumnName == primaryKeyField.Column.ColumnName));
				serverGeneratedPrimaryKey = columns.OfType<ServerGeneratedValue>().FirstOrDefault(q => q.ColumnName == primaryKeyField.Column.ColumnName);
			}

			var manyRelationshipFields = projectionModel.Fields.OfType<IManyRelatedObjectField>().ToArray();

			var bulkInsertRows = new List<QueryExpression[]>();
			var additionalQueries = new List<QueryExpression>();
			foreach (var obj in projections)
			{
				var readWriter = new ObjectReadWriter(obj, typeModel, typeof(TProjection));
				var row = new QueryExpression[columns.Count];
				var i = 0;
				foreach (var column in columns)
				{
					row[i++] = column.GetColumnExpression(readWriter);
				}

				var mapBackPrimaryKey = false;
				if (primaryKeyIsOnProjection && primaryKeyIsServerGenerated)
				{
					mapBackPrimaryKey = !serverGeneratedPrimaryKey.HasValue(readWriter);
				}

				if (mapBackPrimaryKey)
				{
					individualInsertExpressions.Add(new IndividualInsert(new CompositeQueryExpression(new QueryExpression[] {
						QueryExpression.Insert(entityModel.EntityTable.TableName, columns.Select(q => q.ColumnName).ToArray(), row),
						QueryExpression.Select(
							new[] {
								QueryExpression.Alias(QueryExpression.LastInsertIdFunction(), primaryKeyField.Column.ColumnName)
							}
						) }.Concat(CreateManyInserts(QueryExpression.LastInsertIdFunction(), transformer, readWriter, manyRelationshipFields)).ToArray()), readWriter, serverGeneratedPrimaryKey));
				}
				else
				{
					if (primaryKeyIndex > -1)
						additionalQueries.AddRange(CreateManyInserts(row[primaryKeyIndex], transformer, readWriter, manyRelationshipFields));
					bulkInsertRows.Add(row);
				}
			}

			return new InsertOperation(
				bulkInsertRows.Count > 0 ? QueryExpression.Insert(entityModel.EntityTable.TableName, columns.Select(q => q.ColumnName).ToArray(), bulkInsertRows.ToArray()) : null,
				individualInsertExpressions,
				additionalQueries);
		}

		private static IEnumerable<QueryExpression> CreateManyInserts(
			QueryExpression localPrimaryKey, ColumnHelperTransformer transformer,
			IModelReadWriter modelReadWriter, IManyRelatedObjectField[] manyRelatedObjectFields)
		{
			transformer.Current.Clear();
			foreach (var field in manyRelatedObjectFields)
				field.Transform(transformer);
			var helpers = transformer.Current.OfType<MultipleRelationshipReader>();

			foreach (var helper in helpers)
			{
				var rows = new List<QueryExpression[]>();

				foreach (var foreignKeyExpression in helper.GetForeignKeyExpressions(modelReadWriter))
				{
					rows.Add(new QueryExpression[]
					{
						localPrimaryKey,
						foreignKeyExpression
					});
				}

				yield return QueryExpression.Insert(
					helper.Field.JunctionTable.TableName,
					new[] { helper.Field.LocalJunctionColumn.ColumnName, helper.Field.RelatedJunctionColumn.ColumnName },
					rows.ToArray()
					);
			}
		}

		private class IndividualInsert
		{
			public QueryExpression Query { get; }

			private readonly IModelReadWriter _modelReadWriter;
			private readonly ServerGeneratedValue[] _serverGeneratedValues;

			public IndividualInsert(QueryExpression query, IModelReadWriter modelReadWriter,
				params ServerGeneratedValue[] serverGeneratedValues)
			{
				Query = query;
				_modelReadWriter = modelReadWriter;
				_serverGeneratedValues = serverGeneratedValues;
			}

			public void MapOntoObject(QueryResult queryResult)
			{
				foreach (var serverGeneratedValue in _serverGeneratedValues)
				{
					serverGeneratedValue.ReadResultValue(queryResult, _modelReadWriter);
				}
			}
		}
	}
}
