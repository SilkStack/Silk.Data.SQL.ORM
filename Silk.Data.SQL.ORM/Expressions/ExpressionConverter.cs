using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Expressions
{
	public class ExpressionConverter<TSource, TView>
		where TSource : new()
		where TView : new()
	{
		public DataModel<TSource, TView> DataModel { get; }
		private ConverterVisitor _converterVisitor = new ConverterVisitor();

		public ExpressionConverter(DataModel<TSource, TView> dataModel)
		{
			DataModel = dataModel;
		}

		public QueryExpression ConvertToCondition(Expression<Func<TView, bool>> expression)
		{
			var parameterName = expression.Parameters[0].Name;
			_converterVisitor.Setup(parameterName);
			_converterVisitor.Visit(expression.Body);
			return null;
		}

		private class ConverterVisitor : ExpressionVisitor
		{
			private string _parameterName;

			public void Setup(string parameterName)
			{
				_parameterName = parameterName;
			}
		}
	}
}
