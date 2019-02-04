using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class EntitySelectBuilder<T> : QueryBuilderBase<T>, IEntitySelectQueryBuilder<T>
		where T : class
	{
		private IEntityProjectionBuilder<T> _projection;
		public IEntityProjectionBuilder<T> Projection
		{
			get
			{
				if (_projection == null)
					_projection = new DefaultEntityProjectionBuilder<T>(EntitySchema, ExpressionConverter);
				return _projection;
			}
			set => _projection = value;
		}

		private IEntityConditionBuilder<T> _where;
		public IEntityConditionBuilder<T> Where
		{
			get
			{
				if (_where == null)
					_where = new DefaultEntityConditionBuilder<T>(EntitySchema, ExpressionConverter);
				return _where;
			}
			set => _where = value;
		}

		private IEntityConditionBuilder<T> _having;
		public IEntityConditionBuilder<T> Having
		{
			get
			{
				if (_having == null)
					_having = new DefaultEntityConditionBuilder<T>(EntitySchema, ExpressionConverter);
				return _having;
			}
			set => _having = value;
		}

		private IEntityRangeBuilder<T> _range;
		public IEntityRangeBuilder<T> Range
		{
			get
			{
				if (_range == null)
					_range = new DefaultEntityRangeBuilder<T>(EntitySchema, ExpressionConverter);
				return _range;
			}
			set => _range = value;
		}

		private IEntityGroupByBuilder<T> _groupBy;
		public IEntityGroupByBuilder<T> GroupBy
		{
			get
			{
				if (_groupBy == null)
					_groupBy = new DefaultEntityGroupByBuilder<T>(EntitySchema, ExpressionConverter);
				return _groupBy;
			}
			set => _groupBy = value;
		}

		private IEntityOrderByBuilder<T> _orderBy;
		public IEntityOrderByBuilder<T> OrderBy
		{
			get
			{
				if (_orderBy == null)
					_orderBy = new DefaultEntityOrderByBuilder<T>(EntitySchema, ExpressionConverter);
				return _orderBy;
			}
			set => _orderBy = value;
		}

		IConditionBuilder IWhereQueryBuilder.Where { get => Where; set => Where = (IEntityConditionBuilder<T>)value; }
		IConditionBuilder IHavingQueryBuilder.Having { get => Having; set => Having = (IEntityConditionBuilder<T>)value; }
		IGroupByBuilder IGroupByQueryBuilder.GroupBy { get => GroupBy; set => GroupBy = (IEntityGroupByBuilder<T>)value; }
		IOrderByBuilder IOrderByQueryBuilder.OrderBy { get => OrderBy; set => OrderBy = (IEntityOrderByBuilder<T>)value; }
		IRangeBuilder IRangeQueryBuilder.Range { get => Range; set => Range = (IEntityRangeBuilder<T>)value; }

		public EntitySelectBuilder(Schema.Schema schema) : base(schema) { }

		public EntitySelectBuilder(EntitySchema<T> schema) : base(schema) { }

		public override QueryExpression BuildQuery()
		{
			var projection = _projection?.Build();

			if (projection?.ProjectionExpressions.Length == 0)
				throw new InvalidOperationException("At least 1 projected field must be specified.");

			var where = _where?.Build();
			var having = _having?.Build();
			var limit = _range?.BuildLimit();
			var offset = _range?.BuildOffset();
			var groupBy = _groupBy?.Build();
			var orderBy = _orderBy?.Build();

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
				from: Source,
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
