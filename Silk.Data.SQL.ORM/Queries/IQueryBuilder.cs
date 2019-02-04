﻿using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public interface ISelectQueryBuilder : IWhereQueryBuilder, IHavingQueryBuilder, IGroupByQueryBuilder,
		IOrderByQueryBuilder, IRangeQueryBuilder
	{
	}

	public interface IEntitySelectQueryBuilder<T> : ISelectQueryBuilder, IWhereQueryBuilder<T>, IHavingQueryBuilder<T>,
		IGroupByQueryBuilder<T>, IOrderByQueryBuilder<T>, IRangeQueryBuilder<T>
	{
	}

	public interface IQueryBuilder
	{
		QueryExpression BuildQuery();
	}

	public interface IWhereQueryBuilder : IQueryBuilder
	{
		IConditionBuilder Where { get; set; }
	}

	public interface IWhereQueryBuilder<T> : IWhereQueryBuilder
	{
		new IEntityConditionBuilder<T> Where { get; set; }
	}

	public interface IHavingQueryBuilder : IQueryBuilder
	{
		IConditionBuilder Having { get; set; }
	}

	public interface IHavingQueryBuilder<T> : IHavingQueryBuilder
	{
		new IEntityConditionBuilder<T> Having { get; set; }
	}

	public interface IGroupByQueryBuilder : IQueryBuilder
	{
		IGroupByBuilder GroupBy { get; set; }
	}

	public interface IGroupByQueryBuilder<T> : IGroupByQueryBuilder
	{
		new IEntityGroupByBuilder<T> GroupBy { get; set; }
	}

	public interface IOrderByQueryBuilder : IQueryBuilder
	{
		IOrderByBuilder OrderBy { get; set; }
	}

	public interface IOrderByQueryBuilder<T> : IOrderByQueryBuilder
	{
		new IEntityOrderByBuilder<T> OrderBy { get; set; }
	}

	public interface IProjectionQueryBuilder : IQueryBuilder
	{
		IProjectionBuilder Projection { get; set; }
	}

	public interface IProjectionBuilder<T> : IProjectionQueryBuilder
	{
		new IEntityProjectionBuilder<T> Projection { get; set; }
	}

	public interface IRangeQueryBuilder : IQueryBuilder
	{
		IRangeBuilder Range { get; set; }
	}

	public interface IRangeQueryBuilder<T> : IRangeQueryBuilder
	{
		new IEntityRangeBuilder<T> Range { get; set; }
	}
}
