using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	class ProjectionBuilder<TEntity, TProjection>
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
