using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public interface ISelectQueryBuilder : IWhereQueryBuilder, IHavingQueryBuilder, IGroupByQueryBuilder,
		IOrderByQueryBuilder, IRangeQueryBuilder, IProjectionQueryBuilder
	{
	}

	public interface IEntitySelectQueryBuilder<T> : ISelectQueryBuilder, IWhereQueryBuilder<T>, IHavingQueryBuilder<T>,
		IGroupByQueryBuilder<T>, IOrderByQueryBuilder<T>, IRangeQueryBuilder<T>, IEntityQueryBuilder<T>,
		IProjectionQueryBuilder<T>
		where T : class
	{
	}

	public interface IDeleteQueryBuilder : IWhereQueryBuilder
	{
	}

	public interface IEntityDeleteQueryBuilder<T> : IDeleteQueryBuilder, IWhereQueryBuilder<T>, IEntityQueryBuilder<T>
		where T : class
	{
	}

	public interface IUpdateQueryBuilder : IWhereQueryBuilder, IFieldAssignmentQueryBuilder
	{
	}

	public interface IEntityUpdateQueryBuilder<T> : IUpdateQueryBuilder, IWhereQueryBuilder<T>, IFieldAssignmentQueryBuilder<T>, IEntityQueryBuilder<T>
		where T : class
	{
	}

	public interface IInsertQueryBuilder : IFieldAssignmentQueryBuilder
	{
	}

	public interface IEntityInsertQueryBuilder<T> : IInsertQueryBuilder, IFieldAssignmentQueryBuilder<T>, IEntityQueryBuilder<T>
		where T : class
	{
	}

	public interface IQueryBuilder
	{
		QueryExpression BuildQuery();
	}

	public interface IEntityQueryBuilder<T> : IQueryBuilder
		where T : class
	{
	}

	public interface IWhereQueryBuilder
	{
		IConditionBuilder Where { get; set; }
	}

	public interface IWhereQueryBuilder<T> : IWhereQueryBuilder
		where T : class
	{
		new IEntityConditionBuilder<T> Where { get; set; }
	}

	public interface IHavingQueryBuilder
	{
		IConditionBuilder Having { get; set; }
	}

	public interface IHavingQueryBuilder<T> : IHavingQueryBuilder
		where T : class
	{
		new IEntityConditionBuilder<T> Having { get; set; }
	}

	public interface IGroupByQueryBuilder
	{
		IGroupByBuilder GroupBy { get; set; }
	}

	public interface IGroupByQueryBuilder<T> : IGroupByQueryBuilder
		where T : class
	{
		new IEntityGroupByBuilder<T> GroupBy { get; set; }
	}

	public interface IOrderByQueryBuilder
	{
		IOrderByBuilder OrderBy { get; set; }
	}

	public interface IOrderByQueryBuilder<T> : IOrderByQueryBuilder
		where T : class
	{
		new IEntityOrderByBuilder<T> OrderBy { get; set; }
	}

	public interface IProjectionQueryBuilder
	{
		IProjectionBuilder Projection { get; set; }
	}

	public interface IProjectionQueryBuilder<T> : IProjectionQueryBuilder
		where T : class
	{
		new IEntityProjectionBuilder<T> Projection { get; set; }
	}

	public interface IRangeQueryBuilder
	{
		IRangeBuilder Range { get; set; }
	}

	public interface IRangeQueryBuilder<T> : IRangeQueryBuilder
		where T : class
	{
		new IEntityRangeBuilder<T> Range { get; set; }
	}

	public interface IFieldAssignmentQueryBuilder
	{
		IFieldAssignmentBuilder Assignments { get; set; }
	}

	public interface IFieldAssignmentQueryBuilder<T> : IFieldAssignmentQueryBuilder
		where T : class
	{
		new IEntityFieldAssignmentBuilder<T> Assignments { get; set; }
	}
}
