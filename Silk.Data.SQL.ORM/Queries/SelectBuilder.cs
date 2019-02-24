using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class SelectBuilder<T> : IEntitySelectQueryBuilder<T>
		where T : class
	{
		private readonly EntityModel<T> _entityModel;

		public IEntityConditionBuilder<T> Where { get; set; }
		public IEntityConditionBuilder<T> Having { get; set; }
		public IEntityGroupByBuilder<T> GroupBy { get; set; }
		public IEntityOrderByBuilder<T> OrderBy { get; set; }
		public IEntityRangeBuilder<T> Range { get; set; }
		public IEntityProjectionBuilder<T> Projection { get; set; }

		IConditionBuilder IWhereQueryBuilder.Where
		{
			get => Where;
			set => Where = (IEntityConditionBuilder<T>)value;
		}
		IConditionBuilder IHavingQueryBuilder.Having
		{
			get => Having;
			set => Having = (IEntityConditionBuilder<T>)value;
		}
		IGroupByBuilder IGroupByQueryBuilder.GroupBy
		{
			get => GroupBy;
			set => GroupBy = (IEntityGroupByBuilder<T>)value;
		}
		IOrderByBuilder IOrderByQueryBuilder.OrderBy
		{
			get => OrderBy;
			set => OrderBy = (IEntityOrderByBuilder<T>)value;
		}
		IRangeBuilder IRangeQueryBuilder.Range
		{
			get => Range;
			set => Range = (IEntityRangeBuilder<T>)value;
		}
		IProjectionBuilder IProjectionQueryBuilder.Projection
		{
			get => Projection;
			set => Projection = (IEntityProjectionBuilder<T>)value;
		}

		public SelectBuilder(Schema.Schema schema, EntityModel<T> entityModel)
		{
			_entityModel = entityModel;
			var expressionConverter = new EntityExpressionConverter<T>(schema);

			Where = new DefaultEntityConditionBuilder<T>(schema, entityModel, expressionConverter);
			Having = new DefaultEntityConditionBuilder<T>(schema, entityModel, expressionConverter);
			GroupBy = new DefaultEntityGroupByBuilder<T>(schema, entityModel, expressionConverter);
			OrderBy = new DefaultEntityOrderByBuilder<T>(schema, entityModel, expressionConverter);
			Range = new DefaultEntityRangeBuilder<T>(schema, entityModel, expressionConverter);
			Projection = new DefaultEntityProjectionBuilder<T>(schema, entityModel, expressionConverter);
		}

		public SelectBuilder(Schema.Schema schema) : this(schema, schema.GetEntityModel<T>())
		{
		}

		private Join[] ConcatUniqueJoins(params Join[][] joins)
		{
			IEnumerable<Join> result = new Join[0];
			foreach (var joinArray in joins)
			{
				if (joinArray == null || joinArray.Length < 1)
					continue;
				result = result.Concat(joinArray);
			}
			return result
				.GroupBy(join => join)
				.Select(joinGroup => joinGroup.First())
				.ToArray();
		}

		public QueryExpression BuildQuery()
		{
			var projection = Projection.Build();

			if (projection == null || projection.ProjectionExpressions.Length < 1)
				throw new InvalidOperationException("At least 1 projected field must be specified.");

			var where = Where.Build();
			var having = Having.Build();
			var limit = Range.BuildLimit();
			var offset = Range.BuildOffset();
			var groupBy = GroupBy.Build();
			var orderBy = OrderBy.Build();

			var groupByExpressions = groupBy?.Select(q => q.QueryExpression).ToArray();
			var groupByJoins = groupBy?.Where(q => q.RequiredJoins != null).SelectMany(q => q.RequiredJoins).ToArray();

			var orderByExpressions = orderBy?.Select(q => q.QueryExpression).ToArray();
			var orderByJoins = orderBy?.Where(q => q.RequiredJoins != null).SelectMany(q => q.RequiredJoins).ToArray();

			var joins = ConcatUniqueJoins(
				projection?.RequiredJoins,
				where?.RequiredJoins,
				having?.RequiredJoins,
				limit?.RequiredJoins,
				offset?.RequiredJoins,
				groupByJoins,
				orderByJoins
				);

			return QueryExpression.Select(
				projection: projection.ProjectionExpressions,
				from: QueryExpression.Table(_entityModel.Table.TableName),
				joins: joins.Select(q => q.GetJoinExpression()).ToArray(),
				where: where?.QueryExpression,
				having: having?.QueryExpression,
				limit: limit?.QueryExpression,
				offset: offset?.QueryExpression,
				orderBy: orderByExpressions,
				groupBy: groupByExpressions
				);
		}
	}
}
