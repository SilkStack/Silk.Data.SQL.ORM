using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public interface IProjectionBuilder
	{
		//ValueResultMapper<TValue> AddField<TValue>(QueryExpression queryExpression);
		//ObjectResultMapper<TView> AddClass<TView>()
		//	where TView : class;

		Projection Build();
	}

	public interface IEntityProjectionBuilder<T> : IProjectionBuilder
	{
		//ValueResultMapper<TProperty> AddField<TProperty>(Expression<Func<T, TProperty>> projection);
	}

	public class Projection
	{
		public AliasExpression[] ProjectionExpressions { get; }
		public Join[] RequiredJoins { get; }

		public Projection(AliasExpression[] expressions, Join[] joins)
		{
			ProjectionExpressions = expressions;
			RequiredJoins = joins;
		}
	}
}
