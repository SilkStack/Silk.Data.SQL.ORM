using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Expressions
{
	public abstract class ConditionExpression : QueryExpression
	{
		public QueryExpression Expression { get; protected set; }

		public override ExpressionNodeType NodeType => throw new NotImplementedException();
	}

	public class ConditionExpression<TSource, TView> : ConditionExpression
		where TSource : new()
		where TView : new()
	{
		public ConditionExpression(DataModel<TSource, TView> dataModel, Expression<Func<TView, bool>> expression)
		{
			Expression = ConvertToQueryExpression(expression);
		}

		public ConditionExpression<TSource, TView> AndWhere(Expression<Func<TView, bool>> expression)
		{
			Expression = QueryExpression.AndAlso(Expression, ConvertToQueryExpression(expression));
			return this;
		}

		public ConditionExpression<TSource, TView> OrWhere(Expression<Func<TView, bool>> expression)
		{
			Expression = QueryExpression.OrElse(Expression, ConvertToQueryExpression(expression));
			return this;
		}

		private QueryExpression ConvertToQueryExpression(Expression<Func<TView, bool>> expression)
		{
			return null;
		}
	}
}
