using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Expressions
{
	public abstract class ConditionExpression : QueryExpression, IExtensionExpression
	{
		public QueryExpression Expression { get; protected set; }

		public override ExpressionNodeType NodeType => ExpressionNodeType.Extension;

		public void Visit(QueryExpressionVisitor visitor)
		{
			visitor.Visit(Expression);
		}
	}

	public class ConditionExpression<TSource, TView> : ConditionExpression
		where TSource : new()
		where TView : new()
	{
		private readonly ExpressionConverter<TSource, TView> _converter;

		public ConditionExpression(EntityModel<TSource, TView> dataModel, Expression<Func<TView, bool>> expression)
		{
			_converter = new ExpressionConverter<TSource, TView>(dataModel);
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
			return _converter.ConvertToCondition(expression);
		}
	}
}
