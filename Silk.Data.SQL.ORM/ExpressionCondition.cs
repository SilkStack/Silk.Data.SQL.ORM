using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM
{
	public class ExpressionCondition<T> : Condition
	{
		private readonly Expression<Func<T, bool>> _expression;
		private readonly EntityModel<T> _entityModel;

		public ExpressionCondition(Expression<Func<T, bool>> expression, EntityModel<T> entityModel)
		{
			_expression = expression;
			_entityModel = entityModel;
		}

		public override QueryExpression GetExpression()
		{
			//  todo: implement some sort of caching on the expression conversion
			return new ConditionConverter<T>().ConvertToCondition(_expression, _entityModel);
		}
	}
}
