using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Modelling
{
	internal class InsertContainer : IContainer
	{
		public TypedModel Model { get; }

		public IView View { get; }

		public Dictionary<string, QueryExpression> ColumnValues { get; }
			= new Dictionary<string, QueryExpression>();

		public InsertContainer(TypedModel model, IView view)
		{
			Model = model;
			View = view;
		}

		public object GetValue(string[] fieldPath)
		{
			throw new System.NotSupportedException();
		}

		public void SetValue(string[] fieldPath, object value)
		{
			if (fieldPath.Length != 1)
				throw new System.ArgumentOutOfRangeException(nameof(fieldPath), "Field path must have a length of 1.");
			ColumnValues[fieldPath[0]] = QueryExpression.Value(value);
		}
	}
}
