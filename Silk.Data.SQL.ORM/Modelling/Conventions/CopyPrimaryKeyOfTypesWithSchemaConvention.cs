using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using Silk.Data.Modelling.Conventions;
using Silk.Data.SQL.ORM.Modelling.Bindings;
using Silk.Data.SQL.ORM.Modelling.ResourceLoaders;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public class CopyPrimaryKeyOfTypesWithSchemaConvention : ViewConvention
	{
		public override void MakeModelFields(Model model, TypedModelField field, ViewDefinition viewDefinition)
		{
			if (IsSQLType(field.DataType))
				return;

			var bindField = model.Fields.FirstOrDefault(q => q.Name == field.Name);
			if (bindField == null)
				return;

			var dataDomain = viewDefinition.UserData.OfType<DataDomain>()
				.FirstOrDefault();
			if (dataDomain == null)
				return;
			var declaredSchema = dataDomain.GetDeclaredSchema(bindField.DataType);
			if (declaredSchema == null)
				return;

			var primaryKeys = declaredSchema.Fields
				.Where(fieldDefinition => fieldDefinition.Metadata.OfType<PrimaryKeyAttribute>().Any())
				.ToArray();
			foreach (var primaryKey in primaryKeys)
			{
				var fieldName = $"{bindField.Name}{primaryKey.Name}";
				if (viewDefinition.FieldDefinitions.Any(q => q.Name == fieldName))
					continue;

				if (!bindField.CanRead || !bindField.CanWrite)  //  makes no sense to persist data that can't be loaded again, right?
					return;

				//  todo: create a foreign key constraint on this field
				var mappingLoader = GetSubMapper(viewDefinition, dataDomain);
				var mapping = mappingLoader.GetMapping(bindField.DataType);
				mapping.AddField(bindField.Name);
				var fieldDefinition = new ViewFieldDefinition(fieldName,
					new PrimaryKeyBinding(BindingDirection.Bidirectional, new[] { bindField.Name, primaryKey.Name },
						new[] { fieldName }, bindField.Name, new[] { mappingLoader }),
					bindField.Name)
				{
					DataType = primaryKey.DataType
				};
				fieldDefinition.Metadata.AddRange(bindField.Metadata);
				fieldDefinition.Metadata.Add(new IsNullableAttribute(true));
				fieldDefinition.Metadata.Add(new RelationshipDefinition
				{
					Domain = dataDomain,
					EntityType = bindField.DataType,
					RelationshipField = primaryKey.Name
				});
				viewDefinition.GetDefaultTableDefinition()
					.Fields.Add(fieldDefinition);
				viewDefinition.FieldDefinitions.Add(fieldDefinition);
			}
		}

		private JoinedObjectMapper GetSubMapper(ViewDefinition viewDefinition, DataDomain dataDomain)
		{
			var subMapper = viewDefinition.ResourceLoaders
				.OfType<JoinedObjectMapper>()
				.FirstOrDefault();
			if (subMapper == null)
			{
				subMapper = new JoinedObjectMapper(viewDefinition.SourceModel, dataDomain);
				viewDefinition.ResourceLoaders.Add(subMapper);
			}
			return subMapper;
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
