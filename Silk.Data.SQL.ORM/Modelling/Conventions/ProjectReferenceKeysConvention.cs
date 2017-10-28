using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using Silk.Data.Modelling.Conventions;
using Silk.Data.SQL.ORM.Modelling.Bindings;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public class ProjectReferenceKeysConvention : ViewConvention
	{
		public override void MakeModelFields(Model model, TypedModelField field, ViewDefinition viewDefinition)
		{
			var typedModel = model as TypedModel;
			if (typedModel == null)
				return;

			if (!IsSQLType(field.DataType) || viewDefinition.FieldDefinitions.Any(q => q.Name == field.Name))
				return;

			var dataDomain = viewDefinition.UserData.OfType<DataDomain>()
				.FirstOrDefault();
			if (dataDomain == null)
				return;

			var thisDataModel = dataDomain.DataModels.FirstOrDefault(q => q.EntityType == typedModel.DataType);
			if (thisDataModel == null)
				return;

			foreach (var bindField in model.Fields)
			{
				if (IsSQLType(bindField.DataType))
					continue;

				DataField relationshipDatafield = null;
				foreach (var dataField in thisDataModel.Fields)
				{
					if (dataField.Relationship == null)
						continue;
					if (dataField.DataType == field.DataType &&
						field.Name == dataField.Storage.ColumnName)
					{
						relationshipDatafield = dataField;
						break;
					}
				}
				if (relationshipDatafield == null)
					continue;

				var fieldDefinition = new ViewFieldDefinition(field.Name,
					new FlattenedReadBinding(
						new[] { relationshipDatafield.Name, relationshipDatafield.Relationship.ForeignField.Name },
						new[] { field.Name }
					))
				{
					DataType = field.DataType
				};
				fieldDefinition.Metadata.AddRange(field.Metadata);
				viewDefinition.GetDefaultTableDefinition()
					.Fields.Add(fieldDefinition);
				viewDefinition.FieldDefinitions.Add(fieldDefinition);
				break;
			}
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
