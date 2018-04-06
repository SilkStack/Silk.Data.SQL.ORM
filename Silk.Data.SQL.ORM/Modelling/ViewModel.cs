using Silk.Data.SQL.ORM.Schema;
using System;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class ViewModel : ProjectionModel
	{
		public Type ViewType { get; }

		public ViewModel(IEntityField[] fields, Table entityTable, Type viewType) : base(fields, entityTable, null)
		{
			ViewType = viewType;
		}
	}
}
