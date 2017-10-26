using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using Silk.Data.Modelling.Conventions;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public class CopyPrimaryKeyOfTypesWithSchemaConvention : ViewConvention
	{
		public override void MakeModelFields(Model model, TypedModelField field, ViewDefinition viewDefinition)
		{
			if (model == field.ParentModel && !IsSQLType(field.DataType))
			{
				//  creating a view without domain model class
				//  attempt to locate the primary key of the type stored in field
				//  if found create a storage field on the view definition
				//  and a binding that will store the primary key to the created view
				//  and describes how the field on the model actual should be loaded from a db query
				var dataDomain = viewDefinition.UserData.OfType<DataDomain>()
					.FirstOrDefault();
				if (dataDomain == null)
					return;
				var declaredSchema = dataDomain.GetDeclaredSchema(field.DataType);
				if (declaredSchema == null)
					return;

				var primaryKeys = declaredSchema.Fields
					.Where(fieldDefinition => fieldDefinition.Metadata.OfType<PrimaryKeyAttribute>().Any())
					.ToArray();
				foreach (var primaryKey in primaryKeys)
				{
					var fieldName = $"{field.Name}{primaryKey.Name}";
					if (viewDefinition.FieldDefinitions.Any(q => q.Name == fieldName))
						continue;

					if (!field.CanRead || !field.CanWrite)  //  makes no sense to persist data that can't be loaded again, right?
						return;

					//  todo: create a foreign key constraint on this field
					//  todo: replace with a primary key binding
					var fieldDefinition = new ViewFieldDefinition(fieldName,
					new AssignmentBinding(BindingDirection.Bidirectional, new[] { field.Name, primaryKey.Name }, new[] { fieldName }))
					{
						DataType = primaryKey.DataType
					};
					fieldDefinition.Metadata.AddRange(field.Metadata);
					viewDefinition.GetDefaultTableDefinition()
						.Fields.Add(fieldDefinition);
					viewDefinition.FieldDefinitions.Add(fieldDefinition);
				}
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
