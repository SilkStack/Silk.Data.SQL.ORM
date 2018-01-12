using System;
using System.Linq;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class ModelCustomizer<TSource>
		where TSource : new()
	{
		private DataViewBuilder _viewBuilder;

		public ModelCustomizer(DataViewBuilder viewBuilder)
		{
			_viewBuilder = viewBuilder;
		}

		public FieldCustomizer<TField> For<TField>(Expression<Func<TSource, TField>> fieldSelector)
		{
			var memberName = ((MemberExpression)fieldSelector.Body).Member.Name;
			var field = _viewBuilder.ViewDefinition.FieldDefinitions.FirstOrDefault(q => q.Name == memberName);
			if (field == null)
				return null; //  todo: replace with an appropriate exception?
			return new FieldCustomizer<TField>(_viewBuilder, field);
		}
	}
}
