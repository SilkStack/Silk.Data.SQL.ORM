using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Operations
{
	public class SelectOperation : DataOperation
	{
		public static SelectOperation Create<TEntity>(EntityModel<TEntity> model)
		{
			return Create(model);
		}

		public static SelectOperation Create<TEntity, TProjection>(EntityModel<TEntity> model)
		{
			return Create(model.GetProjection<TProjection>());
		}

		private static SelectOperation Create(ProjectionModel model)
		{
			var entityTableExpr = QueryExpression.Table(model.EntityTable.TableName);
			var query = CreateQuery(model, entityTableExpr);

			return null;
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

			return null;
		}

		private static void AddFields(ProjectionModel model, List<QueryExpression> projectedFieldsExprs, QueryExpression from, List<JoinExpression> joins)
		{
			foreach (var field in model.Fields)
			{
				if (field is ValueField valueField)
				{
					projectedFieldsExprs.Add(
						QueryExpression.Alias(
							QueryExpression.Column(valueField.Column.ColumnName, from),
							valueField.FieldName
						));
				}
				else if (field is SingleRelatedObjectField singleRelationshipField)
				{
					var joinAlias = QueryExpression.Alias(
						QueryExpression.Table(singleRelationshipField.RelatedObjectModel.EntityTable.TableName),
						singleRelationshipField.FieldName
						);
					var joinExpr = QueryExpression.Join(
						QueryExpression.Column(singleRelationshipField.SqlFieldName, from),
						QueryExpression.Column(singleRelationshipField.RelatedPrimaryKey.Column.ColumnName, joinAlias),
						JoinDirection.Left
						);

					joins.Add(joinExpr);
					AddFields(singleRelationshipField.RelatedObjectModel, projectedFieldsExprs, joinAlias, joins);
				}
			}
		}
	}
}
