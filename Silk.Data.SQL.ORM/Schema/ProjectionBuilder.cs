using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	public static class ProjectionBuilder
	{
		private static IEnumerable<ISourceField> AllFields(SourceModel fromModel)
		{
			foreach (var field in fromModel.Fields)
			{
				yield return field;
				foreach (var subField in field.Fields.SelectMany(q => AllFields(q)))
				{
					yield return subField;
				}
			}
		}

		private static IEnumerable<ISourceField> AllFields(ISourceField sourceField)
		{
			yield return sourceField;
			foreach (var field in sourceField.Fields.SelectMany(q => AllFields(q)))
			{
				yield return field;
			}
		}

		public static IEnumerable<(ISourceField sourceField, ITargetField targetField)> GetBindCandidatePairs(SourceModel fromModel, TargetModel toModel, MappingBuilder builder)
		{
			//  find fields with matching names that aren't already bound
			foreach (var fromField in AllFields(fromModel).Where(q => q.CanRead && !builder.IsBound(q)))
			{
				var toField = toModel.Fields.FirstOrDefault(q => q.CanWrite && string.Join("", q.FieldPath) == fromField.FieldName);
				if (toField == null)
					continue;

				if (!builder.IsBound(fromField) && !builder.IsBound(toField))
					yield return (fromField, toField);
			}

			//  flattening candidates from the target model
			foreach (var toField in toModel.Fields.Where(q => q.CanWrite && !builder.IsBound(q)))
			{
				var potentialPaths = ConventionUtilities.GetPaths(toField.FieldName).ToArray();
				if (potentialPaths.Length == 1)
					continue;

				ISourceField fromField = null;
				foreach (var sourcePath in potentialPaths)
				{
					var testField = fromModel.GetField(sourcePath);
					if (testField == null || !testField.CanRead)
						continue;
					fromField = testField;
					break;
				}
				if (fromField == null)
					continue;

				if (!builder.IsBound(fromField) && !builder.IsBound(toField))
					yield return (fromField, toField);
			}

			//  inflation candidates from the source model
			foreach (var fromField in AllFields(fromModel).Where(q => q.CanRead))
			{
				var potentialPaths = ConventionUtilities.GetPaths(fromField.FieldName)
					.Concat(ConventionUtilities.GetPaths(string.Join("", fromField.FieldPath)))
					.ToArray();
				if (potentialPaths.Length == 1)
					continue;

				ITargetField toField = null;

				foreach (var targetPath in potentialPaths)
				{
					var testField = toModel.GetField(targetPath);
					if (testField == null || !testField.CanWrite || testField.FieldType != fromField.FieldType ||
						builder.IsBound(testField))
						continue;
					toField = testField;
					break;
				}
				if (toField == null)
					continue;

				if (!builder.IsBound(fromField) && !builder.IsBound(toField))
					yield return (fromField, toField);
			}
		}
	}

	public class ProjectionBuilder<TEntity, TProjection>
		where TEntity : class
		where TProjection : class
	{
		private readonly static TypeModel<TEntity> _entityModel
			= TypeModel.GetModelOf<TEntity>();
		private readonly static IFieldResolver _entityFieldResolver
			= _entityModel.CreateFieldResolver();

		public ProjectionSchema<TProjection, TEntity> Build(EntitySchema<TEntity> entitySchema)
		{
			var mapping = GetMapping(
				entitySchema.SchemaModel, typeof(TProjection),
				entitySchema.Schema.ProjectionMappingOptions);
			var mappingConverter = new MappingToFieldsConverter(entitySchema);

			var schemaFields = mappingConverter.Convert(mapping);
			return new ProjectionSchema<TProjection, TEntity>(
					entitySchema.EntityTable,
					schemaFields,
					entitySchema.Indexes,
					mapping
					);
		}

		private readonly static object _syncObject = new object();
		private readonly static MappingStore _mappingStore = new MappingStore();

		private static Mapping GetMapping(SchemaModel fromModel, Type projectionType,
			MappingOptions options)
		{
			var toModel = TypeModel.GetModelOf(projectionType);
			if (_mappingStore.TryGetMapping(fromModel, toModel, out var mapping))
				return mapping;

			lock (_syncObject)
			{
				if (_mappingStore.TryGetMapping(fromModel, toModel, out mapping))
					return mapping;

				var mappingBuilder = new MappingBuilder(fromModel, toModel, options, _mappingStore);
				return mappingBuilder.BuildMapping();
			}
		}

		private class MappingToFieldsConverter
		{
			private readonly EntitySchema<TEntity> _entitySchema;

			public MappingToFieldsConverter(EntitySchema<TEntity> entitySchema)
			{
				_entitySchema = entitySchema;
			}

			public ISchemaField<TEntity>[] Convert(Mapping mapping)
			{
				var schemaFields = new List<ISchemaField<TEntity>>();

				foreach (var binding in mapping.Bindings)
				{
					Visit(binding, schemaFields);
				}
				return schemaFields.ToArray();
			}

			private void Visit(Modelling.Mapping.Binding.Binding binding, List<ISchemaField<TEntity>> schemaFields)
			{
				switch (binding)
				{
					case SubmappingBindingBase submappingBinding:
						break;
					case MappingBinding mappingBinding:
						var mappingField = CreateMappingField(mappingBinding);
						schemaFields.Add(mappingField);
						break;
				}
			}

			private ISchemaField<TEntity> CreateMappingField(MappingBinding mappingBinding)
			{
				var sourceField = _entitySchema.SchemaFields.FirstOrDefault(q => ReferenceEquals(q.SchemaFieldReference, mappingBinding.From));

				return new ProjectedPrimitiveSchemaField<TEntity>(
					sourceField.FieldName, sourceField.Column, sourceField.PrimaryKeyGenerator,
					sourceField.EntityFieldReference, sourceField.Join,
					sourceField.ModelPath, sourceField.AliasName,
					sourceField.DataType, sourceField.SchemaFieldReference,
					new Modelling.Mapping.Binding.Binding[]
					{
						mappingBinding
					});
			}
		}
	}
}
