using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Modelling
{
	//internal class UpdateContainer : IContainer
	//{
	//	public TypedModel Model { get; }

	//	public IView View { get; }

	//	public Dictionary<string, AssignColumnExpression> AssignExpressions { get; }
	//		= new Dictionary<string, AssignColumnExpression>();

	//	private Dictionary<string, object> _values = new Dictionary<string, object>();

	//	public UpdateContainer(TypedModel model, IView view)
	//	{
	//		Model = model;
	//		View = view;
	//	}

	//	public object GetValue(string[] fieldPath)
	//	{
	//		if (fieldPath.Length != 1)
	//			throw new System.ArgumentOutOfRangeException(nameof(fieldPath), "Field path must have a length of 1.");
	//		return _values[fieldPath[0]];
	//	}

	//	public void SetValue(string[] fieldPath, object value)
	//	{
	//		if (fieldPath.Length != 1)
	//			throw new System.ArgumentOutOfRangeException(nameof(fieldPath), "Field path must have a length of 1.");
	//		AssignExpressions[fieldPath[0]] = QueryExpression.Assign(fieldPath[0], value);
	//		_values[fieldPath[0]] = value;
	//	}
	//}
}
