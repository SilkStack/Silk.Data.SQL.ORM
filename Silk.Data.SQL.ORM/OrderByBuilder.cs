using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM
{
	public abstract class OrderByBuilderBase
	{
		private readonly List<OrderBy> statements = new List<OrderBy>();

		protected void Add(OrderBy orderBy)
		{
			statements.Add(orderBy);
		}

		public OrderBy[] Build()
		{
			return statements.ToArray();
		}
	}

	public class OrderByBuilder : OrderByBuilderBase
	{
		public OrderByBuilder ThenBy(QueryExpression queryExpression)
		{
			Add(new QueryExpressionOrderBy(queryExpression));
			return this;
		}

		public OrderByBuilder ThenByDescending(QueryExpression queryExpression)
		{
			Add(new QueryExpressionOrderBy(QueryExpression.Descending(queryExpression)));
			return this;
		}
	}

	public class OrderByBuilder<T> : OrderByBuilderBase
	{
		private readonly EntityModel<T> _entityModel;

		public OrderByBuilder(EntityModel<T> entityModel)
		{
			_entityModel = entityModel;
		}

		public OrderByBuilder<T> ThenBy(QueryExpression queryExpression)
		{
			Add(new QueryExpressionOrderBy(queryExpression));
			return this;
		}

		public OrderByBuilder<T> ThenByDescending(QueryExpression queryExpression)
		{
			Add(new QueryExpressionOrderBy(QueryExpression.Descending(queryExpression)));
			return this;
		}

		public OrderByBuilder<T> ThenBy<TProperty>(Expression<Func<T,TProperty>> propertySelector)
		{
			return ThenBy(new PropertyConverter<T>().ConvertToProperty(propertySelector, _entityModel));
		}

		public OrderByBuilder<T> ThenByDescending<TProperty>(Expression<Func<T, TProperty>> propertySelector)
		{
			return ThenByDescending(new PropertyConverter<T>().ConvertToProperty(propertySelector, _entityModel));
		}
	}
}
