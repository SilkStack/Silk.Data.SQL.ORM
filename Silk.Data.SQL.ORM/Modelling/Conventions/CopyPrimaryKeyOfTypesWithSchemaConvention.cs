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
					var mappingLoader = GetSubMapper(viewDefinition, dataDomain);
					var mapping = mappingLoader.GetMapping(field.DataType);
					mapping.AddField(field.Name);
					var fieldDefinition = new ViewFieldDefinition(fieldName,
						new PrimaryKeyBinding(BindingDirection.Bidirectional, new[] { field.Name, primaryKey.Name },
							new[] { fieldName }, field.Name),
						field.Name)
					{
						DataType = primaryKey.DataType
					};
					fieldDefinition.Metadata.AddRange(field.Metadata);
					fieldDefinition.Metadata.Add(new IsNullableAttribute(true));
					fieldDefinition.Metadata.Add(new RelationshipDefinition
					{
						Domain = dataDomain,
						EntityType = field.DataType,
						RelationshipField = primaryKey.Name
					});
					viewDefinition.GetDefaultTableDefinition()
						.Fields.Add(fieldDefinition);
					viewDefinition.FieldDefinitions.Add(fieldDefinition);
				}
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
