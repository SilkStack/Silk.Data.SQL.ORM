using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
using Silk.Data.SQL.ORM.Schema;
using System;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class ViewModel : IProjectionModel
	{
		public Type ViewType { get; }

		public Table EntityTable { get; }

		public Mapping Mapping { get; }

		public IEntityField[] Fields { get; }

		IField[] IModel.Fields => Fields;

		public ViewModel(IEntityField[] fields, Table entityTable, Type viewType)
		{
			Fields = fields;
			EntityTable = entityTable;
			ViewType = viewType;
		}

		public AssignmentBinding GetCreateInstanceAsNeededBinding(string[] path)
		{
			throw new NotImplementedException();
		}

		public void Transform(IModelTransformer transformer)
		{
			throw new NotImplementedException();
		}
	}
}
