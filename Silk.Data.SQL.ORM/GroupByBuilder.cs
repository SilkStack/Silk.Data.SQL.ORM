using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM
{
	public abstract class GroupByBuilderBase
	{
		private readonly List<GroupBy> statements = new List<GroupBy>();

		protected void Add(GroupBy groupBy)
		{
			statements.Add(groupBy);
		}

		public GroupBy[] Build()
		{
			return statements.ToArray();
		}
	}

	public class GroupByBuilder : GroupByBuilderBase
	{
		public GroupByBuilder ThenBy(QueryExpression queryExpression)
		{
			Add(new QueryExpressionGroupBy(queryExpression));
			return this;
		}
	}

	public class GroupByBuilder<T> : GroupByBuilderBase
	{
		private readonly EntityModel<T> _entityModel;

		public GroupByBuilder(EntityModel<T> entityModel)
		{
			_entityModel = entityModel;
		}

		public GroupByBuilder<T> ThenBy(QueryExpression queryExpression)
		{
			Add(new QueryExpressionGroupBy(queryExpression));
			return this;
		}

		public GroupByBuilder<T> ThenBy<TProperty>(Expression<Func<T, TProperty>> propertySelector)
		{
			return ThenBy(new PropertyConverter<T>().ConvertToProperty(propertySelector, _entityModel));
		}
	}
}
