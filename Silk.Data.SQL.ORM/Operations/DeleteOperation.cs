using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.Queries;

namespace Silk.Data.SQL.ORM.Operations
{
	public class DeleteOperation : DataOperation
	{
		private readonly QueryExpression _queryExpression;

		public override bool CanBeBatched => true;

		public DeleteOperation(QueryExpression queryExpression)
		{
			_queryExpression = queryExpression;
		}

		public override QueryExpression GetQuery() => _queryExpression;

		public override void ProcessResult(QueryResult queryResult)
		{
		}

		public override Task ProcessResultAsync(QueryResult queryResult) => Task.CompletedTask;

		public static DeleteOperation Create<TEntity>(EntityModel<TEntity> model, params TEntity[] entities)
			where TEntity : class
		{
			var primaryKeyFields = model.Fields.OfType<IValueField>().Where(q => q.Column.IsPrimaryKey)
				.ToArray();
			if (primaryKeyFields.Length == 0)
				throw new InvalidOperationException("Model requires at least 1 primary key field to generate entity DELETE statements.");

			var transformer = new ColumnHelperTransformer(model);
			var entityTypeModel = TypeModel.GetModelOf<TEntity>();
			foreach (var pkField in primaryKeyFields)
			{
				pkField.Transform(transformer);
			}
			var primaryKeyHelpers = transformer.Current;
			if (primaryKeyHelpers.Count != primaryKeyFields.Length)
				throw new Exception("Failed to transform all primary keys to helper fields.");

			var conditionBuilder = new ConditionBuilder();
			foreach (var entity in entities)
			{
				var entityConditionBuilder = new ConditionBuilder();
				var objectReadWriter = new ObjectReadWriter(entity, entityTypeModel, typeof(TEntity));

				foreach (var field in primaryKeyHelpers)
				{
					entityConditionBuilder.And(new QueryExpressionCondition(
						QueryExpression.Compare(
							QueryExpression.Column(field.ColumnName),
							ComparisonOperator.AreEqual,
							field.GetColumnExpression(objectReadWriter))
						));
				}

				conditionBuilder.Or(entityConditionBuilder.Build());
			}

			return new DeleteOperation(QueryExpression.Delete(
				QueryExpression.Table(model.EntityTable.TableName),
				conditionBuilder.Build().GetExpression()
				));
		}
	}
}
