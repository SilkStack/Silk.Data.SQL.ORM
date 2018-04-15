using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Operations.Expressions;
using Silk.Data.SQL.Queries;

namespace Silk.Data.SQL.ORM.Operations
{
	public class UpdateOperation : DataOperation
	{
		private readonly QueryExpression _queryExpression;

		public override bool CanBeBatched => true;

		public UpdateOperation(QueryExpression queryExpression)
		{
			_queryExpression = queryExpression;
		}

		public override QueryExpression GetQuery() => _queryExpression;

		public override void ProcessResult(QueryResult queryResult) { }

		public override Task ProcessResultAsync(QueryResult queryResult) => Task.CompletedTask;

		public static UpdateOperation Create<TEntity>(EntityModel<TEntity> model, params TEntity[] entities)
			where TEntity : class
		{
			return Create(model, model, entities);
		}

		public static UpdateOperation Create<TEntity, TProjection>(EntityModel<TEntity> model, params TProjection[] projections)
			where TProjection : class
		{
			return Create<TProjection>(model.GetProjection<TProjection>(), model, projections);
		}

		public static UpdateOperation Create<TEntity, TProjection>(EntityModel<TEntity> model, TProjection projection, Condition condition)
			where TProjection : class
		{
			var projectionModel = model.GetProjection<TProjection>();
			var objectReadWriter = new ObjectReadWriter(projection, TypeModel.GetModelOf<TProjection>(), typeof(TProjection));
			var columns = GetFields(projectionModel);
			return new UpdateOperation(CreateUpdateExpression<TProjection>(
				projectionModel, model, objectReadWriter, condition, columns
				));
		}

		private static List<ColumnHelper> GetFields(ProjectionModel projectionModel, ColumnHelperTransformer transformer = null)
		{
			if (transformer == null)
				transformer = new ColumnHelperTransformer(projectionModel);
			var columns = new List<ColumnHelper>();
			foreach (var projectionField in projectionModel.Fields)
			{
				if (projectionField is IManyRelatedObjectField)
					continue;
				projectionField.Transform(transformer);
			}
			columns.AddRange(transformer.Current.Where(q => q != null));
			return columns;
		}

		private static UpdateOperation Create<TProjection>(ProjectionModel projectionModel, EntityModel entityModel, params TProjection[] projections)
			where TProjection : class
		{
			var primaryKeyFields = entityModel.Fields.OfType<IValueField>().Where(q => q.Column.IsPrimaryKey)
				.ToArray();
			if (primaryKeyFields.Length == 0)
				throw new InvalidOperationException("Model requires at least 1 primary key field to generate entity UPDATE statements.");

			var transformer = new ColumnHelperTransformer(projectionModel);
			foreach (var pkField in primaryKeyFields)
			{
				pkField.Transform(transformer);
			}
			var primaryKeyHelpers = transformer.Current.ToArray();
			transformer.Current.Clear();
			if (primaryKeyHelpers.Length != primaryKeyFields.Length)
				throw new Exception("Failed to transform all primary keys to helper fields.");

			var columns = GetFields(projectionModel, transformer);

			var updateExpressions = new List<QueryExpression>();
			var typeModel = TypeModel.GetModelOf<TProjection>();
			foreach (var obj in projections)
			{
				var entityConditionBuilder = new ConditionBuilder();
				var objectReadWriter = new ObjectReadWriter(obj, typeModel, typeof(TProjection));

				foreach (var field in primaryKeyHelpers)
				{
					entityConditionBuilder.And(new QueryExpressionCondition(
						QueryExpression.Compare(
							QueryExpression.Column(field.ColumnName),
							ComparisonOperator.AreEqual,
							field.GetColumnExpression(objectReadWriter))
						));
				}

				updateExpressions.Add(CreateUpdateExpression<TProjection>(
					projectionModel, entityModel, objectReadWriter,
					entityConditionBuilder.Build(), columns
					));
			}
			return new UpdateOperation(new CompositeQueryExpression(updateExpressions.ToArray()));
		}

		private static QueryExpression CreateUpdateExpression<TProjection>(ProjectionModel projectionModel, EntityModel entityModel, IModelReadWriter objectReadWriter, Condition condition,
			List<ColumnHelper> columns)
		{
			var row = new QueryExpression[columns.Count];
			var i = 0;
			foreach (var field in columns)
			{
				row[i++] = field.GetColumnExpression(objectReadWriter);
			}
			return QueryExpression.Update(
				QueryExpression.Table(entityModel.EntityTable.TableName),
				where: condition.GetExpression(),
				assignments: columns
					.Zip(row, (column,val) => QueryExpression.Assign(QueryExpression.Column(column.ColumnName), val))
					.ToArray()
				);
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

				yield return new CompositeQueryExpression(
					QueryExpression.Delete(
						QueryExpression.Table(helper.Field.JunctionTable.TableName),
						QueryExpression.Compare(QueryExpression.Column(helper.Field.LocalJunctionColumn.ColumnName), ComparisonOperator.AreEqual, localPrimaryKey)
						),
					QueryExpression.Insert(
						helper.Field.JunctionTable.TableName,
						new[] { helper.Field.LocalJunctionColumn.ColumnName, helper.Field.RelatedJunctionColumn.ColumnName },
						rows.ToArray()
						)
					);
			}
		}
	}
}
