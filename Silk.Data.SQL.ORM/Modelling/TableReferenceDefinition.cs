using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class TableReferenceDefinition
	{
		public List<ReferencedViewDefinition> EntityTableReferences { get; } = new List<ReferencedViewDefinition>();

		public void AddReferenceToEntityTable(ViewBuilder.FieldInfo sourceField, TableDefinition foreignTable, ViewFieldDefinition foreignTableField)
		{
			EntityTableReferences.Add(new ReferencedViewDefinition(
				foreignTable, $"{sourceField.Field.Name}{foreignTableField.Name}",
				new AssignmentBinding(BindingDirection.ModelToView,
					new[] { sourceField.Field.Name, foreignTableField.Name },
					new[] { $"{sourceField.Field.Name}{foreignTableField.Name}" })
				) { DataType = foreignTableField.DataType });
		}
	}

	public class ReferencedViewDefinition : ViewFieldDefinition
	{
		public ReferencedViewDefinition(TableDefinition table, string name, ModelBinding binding, string modelFieldName = null)
			: base(name, binding, modelFieldName)
		{
			Table = table;
		}

		public TableDefinition Table { get; }
	}
}
