using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using Silk.Data.Modelling.Conventions;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public class CopySupportedSQLTypesConvention : ViewConvention
	{
		public override void MakeModelFields(Model model, TypedModelField field, ViewDefinition viewDefinition)
		{
			if (!IsSQLType(field.DataType) || viewDefinition.FieldDefinitions.Any(q => q.Name == field.Name))
				return;
			var bindField = model.Fields.FirstOrDefault(q => q.Name == field.Name && q.DataType == field.DataType);
			if (bindField == null)
				return;

			var bindingDirection = BindingDirection.None;
			if (field.CanWrite && bindField.CanRead)
				bindingDirection |= BindingDirection.ModelToView;
			if (field.CanRead && bindField.CanWrite)
				bindingDirection |= BindingDirection.ViewToModel;
			if (bindingDirection == BindingDirection.None)
				return;

			var fieldDefinition = new ViewFieldDefinition(field.Name,
					new AssignmentBinding(bindingDirection, new[] { bindField.Name }, new[] { field.Name }))
			{
				DataType = field.DataType
			};
			fieldDefinition.Metadata.AddRange(field.Metadata);
			viewDefinition.GetEntityTableDefinition()
				.Fields.Add(fieldDefinition);
			viewDefinition.FieldDefinitions.Add(fieldDefinition);
		}

		private static bool IsSQLType(Type type)
		{
			return
				type == typeof(bool) ||
				type == typeof(byte) ||
				type == typeof(short) ||
				type == typeof(int) ||
				type == typeof(long) ||
				type == typeof(float) ||
				type == typeof(double) ||
				type == typeof(decimal) ||
				type == typeof(string) ||
				type == typeof(Guid) ||
				type == typeof(DateTime) ||
				type == typeof(byte[])
				;
		}
	}
}
