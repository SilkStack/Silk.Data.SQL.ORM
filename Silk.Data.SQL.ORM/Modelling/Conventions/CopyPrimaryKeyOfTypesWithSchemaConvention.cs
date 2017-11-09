﻿using Silk.Data.Modelling;
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
			if (IsSQLType(field.DataType) || viewDefinition.FieldDefinitions.Any(q => q.Name == field.Name))
				return;

			var bindField = model.Fields.FirstOrDefault(q => q.Name == field.Name);
			if (bindField == null)
				return;

			var bindToEntityTable = viewDefinition.UserData.OfType<DomainDefinition>()
				.First().SchemaDefinitions.FirstOrDefault(q => q.EntityType == bindField.DataType)
				?.TableDefinitions.FirstOrDefault(q => q.IsEntityTable);
			if (bindToEntityTable == null)
				return;

			var dataDomain = viewDefinition.UserData.OfType<Lazy<DataDomain>>()
				.FirstOrDefault();
			if (dataDomain == null)
				return;

			var primaryKeys = bindToEntityTable.Fields
				.Where(fieldDefinition => fieldDefinition.Metadata.OfType<PrimaryKeyAttribute>().Any())
				.ToArray();

			if (bindField.IsEnumerable)
			{
				//  if the foreign datatype has fields that reference this datatype then it's a one-to-many relationship
				//  ie. all fields of local datatype on the foreign datatype/model populate `field`
				//  if the foreign datatype doesn't reference local datatype, or references as an enumerable
				//  create a centralized many-to-many relationship
				return;
			}

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
				mapping.AddField(bindField.Name, field.DataType);
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
					EntityType = bindField.DataType,
					ProjectionType = field.DataType,
					RelationshipField = primaryKey.Name,
					RelationshipType = RelationshipType.ManyToOne
				});
				if (!viewDefinition.UserData.OfType<DomainDefinition>().First().IsReadOnly)
					viewDefinition.GetEntityTableDefinition()
						.Fields.Add(fieldDefinition);
				viewDefinition.FieldDefinitions.Add(fieldDefinition);
			}
		}

		private JoinedObjectMapper GetSubMapper(ViewDefinition viewDefinition, Lazy<DataDomain> dataDomain)
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
