using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public interface ISelectQueryBuilder : IWhereQueryBuilder, IHavingQueryBuilder, IGroupByQueryBuilder,
		IOrderByQueryBuilder, IRangeQueryBuilder
	{
	}

	public interface IEntitySelectQueryBuilder<T> : ISelectQueryBuilder, IWhereQueryBuilder<T>, IHavingQueryBuilder<T>,
		IGroupByQueryBuilder<T>, IOrderByQueryBuilder<T>, IRangeQueryBuilder<T>
		where T : class
	{
	}

	public interface IDeleteQueryBuilder : IWhereQueryBuilder
	{
	}

	public interface IEntityDeleteQueryBuilder<T> : IDeleteQueryBuilder, IWhereQueryBuilder<T>
		where T : class
	{
	}

	public interface IUpdateQueryBuilder : IWhereQueryBuilder, IFieldAssignmentQueryBuilder
	{
	}

	public interface IEntityUpdateQueryBuilder<T> : IUpdateQueryBuilder, IWhereQueryBuilder<T>, IFieldAssignmentQueryBuilder<T>
		where T : class
	{
	}

	public interface IInsertQueryBuilder : IFieldAssignmentQueryBuilder
	{
	}

	public interface IEntityInsertQueryBuilder<T> : IInsertQueryBuilder, IFieldAssignmentQueryBuilder<T>
		where T : class
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
		where T : class
	{
		new IEntityConditionBuilder<T> Where { get; set; }
	}

	public interface IHavingQueryBuilder : IQueryBuilder
	{
		IConditionBuilder Having { get; set; }
	}

	public interface IHavingQueryBuilder<T> : IHavingQueryBuilder
		where T : class
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

	public interface IProjectionQueryBuilder<T> : IProjectionQueryBuilder
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

	public interface IFieldAssignmentQueryBuilder : IQueryBuilder
	{
		IFieldAssignmentBuilder Assignments { get; set; }
	}

	public interface IFieldAssignmentQueryBuilder<T> : IFieldAssignmentQueryBuilder
		where T : class
	{
		new IEntityFieldAssignmentBuilder<T> Assignments { get; set; }
	}
}
