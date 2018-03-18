using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Operations
{
	public class SelectResult<T>
	{
	}

	public class SelectOperation<T> : DataOperationWithResult<SelectResult<T>>
	{
		private readonly QueryExpression _query;

		public override SelectResult<T> Result => throw new System.NotImplementedException();

		public override bool CanBeBatched => true;

		public SelectOperation(QueryExpression query)
		{
			_query = query;
		}

		public override QueryExpression GetQuery()
		{
			return _query;
		}

		public override void ProcessResult(QueryResult queryResult)
		{
			throw new System.NotImplementedException();
		}

		public override Task ProcessResultAsync(QueryResult queryResult)
		{
			throw new System.NotImplementedException();
		}
	}

	public class SelectOperation
	{
		public static SelectOperation<TEntity> Create<TEntity>(EntityModel<TEntity> model)
		{
			return Create<TEntity>((ProjectionModel)model);
		}

		public static SelectOperation<TProjection> Create<TEntity, TProjection>(EntityModel<TEntity> model)
		{
			return Create<TProjection>(model.GetProjection<TProjection>());
		}

		private static SelectOperation<TProjection> Create<TProjection>(ProjectionModel model)
		{
			var entityTableExpr = QueryExpression.Table(model.EntityTable.TableName);
			var query = CreateQuery(model, entityTableExpr);
			return new SelectOperation<TProjection>(query);
		}

		private static QueryExpression CreateQuery(ProjectionModel model, QueryExpression from)
		{
			var projectedFieldsExprs = new List<QueryExpression>();
			var joinExprs = new List<JoinExpression>();

			AddFields(model, projectedFieldsExprs, from, joinExprs);

			var query = QueryExpression.Select(
				projectedFieldsExprs.ToArray(),
				from: from,
				joins: joinExprs.Count < 1 ? null : joinExprs.ToArray()
				);

			foreach (var field in model.Fields.OfType<ManyRelatedObjectField>())
			{

			}

			return query;
		}

		private static void AddFields(ProjectionModel model, List<QueryExpression> projectedFieldsExprs, QueryExpression from, List<JoinExpression> joins,
			string fieldPrefix = "")
		{
			foreach (var field in model.Fields)
			{
				if (field is ValueField valueField)
				{
					var fieldSource = from;
					if (fieldSource is AliasExpression aliasExpression)
						fieldSource = aliasExpression.Identifier;
					projectedFieldsExprs.Add(
						QueryExpression.Alias(
							QueryExpression.Column(valueField.Column.ColumnName, fieldSource),
							$"{fieldPrefix}{valueField.FieldName}"
						));
				}
				else if (field is SingleRelatedObjectField singleRelationshipField)
				{
					var joinAlias = QueryExpression.Alias(
						QueryExpression.Table(singleRelationshipField.RelatedObjectModel.EntityTable.TableName),
						singleRelationshipField.FieldName
						);
					var joinExpr = QueryExpression.Join(
						QueryExpression.Column(singleRelationshipField.LocalColumn.ColumnName, from),
						QueryExpression.Column(singleRelationshipField.RelatedPrimaryKey.Column.ColumnName, joinAlias),
						JoinDirection.Left
						);

					joins.Add(joinExpr);
					AddFields(singleRelationshipField.RelatedObjectModel, projectedFieldsExprs, joinAlias, joins, $"{fieldPrefix}{field.FieldName}_");
				}
			}
		}
	}
}
