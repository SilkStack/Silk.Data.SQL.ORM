using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	public class SchemaModel : SourceModel
	{
		public SchemaModel(IModel fromModel, ISourceField[] fields, string[] selfPath, IModel rootModel = null)
			: base(fromModel, fields, selfPath, rootModel)
		{
		}

		public override IFieldResolver CreateFieldResolver()
		{
			return new FieldResolver();
		}

		public static SchemaModel Create<T>(EntitySchema<T> entitySchema)
			where T : class
		{
			var typeModel = TypeModel.GetModelOf<T>();
			var transformer = new Transformer<T>(null, typeModel, entitySchema);
			typeModel.Transform(transformer);
			return transformer.BuildSchemaModel();
		}

		private class FieldResolver : IFieldResolver
		{
			public void AddMutator(IFieldReferenceMutator mutator)
			{
			}

			public void RemoveMutator(IFieldReferenceMutator mutator)
			{
			}

			public ModelNode ResolveNode(IFieldReference fieldReference)
			{
				if (!(fieldReference is ISchemaFieldReference schemaFieldReference))
					throw new InvalidOperationException($"Unsupported field reference type: {fieldReference.GetType().FullName}");
				return new ModelNode(
					schemaFieldReference.Field,
					new ModelPathNode[] { new FieldPathNode(schemaFieldReference.FieldAlias, schemaFieldReference.Field) }
					);
			}
		}

		public class Transformer<T> : IModelTransformer
			where T : class
		{
			private IModel _fromModel;
			private readonly List<ISourceField> _fields = new List<ISourceField>();
			private readonly string[] _rootPath;
			private readonly IModel _rootModel;
			private readonly EntitySchema<T> _entitySchema;

			public Transformer(string[] rootPath, IModel rootModel, EntitySchema<T> entitySchema)
			{
				_rootPath = rootPath ?? new string[0];
				_rootModel = rootModel;
				_entitySchema = entitySchema;
			}

			public void VisitModel<TField>(IModel<TField> model) where TField : IField
			{
				_fromModel = model;
			}

			public void VisitField<T1>(IField<T1> field)
			{
				var schemaFieldReference = FindSchemaFieldReference(field);
				if (schemaFieldReference == null && SqlTypeHelper.IsSqlPrimitiveType(field.FieldType))
					return;

				if (_rootPath == null || _rootPath.Length == 0)
				{
					_fields.Add(new SchemaField<T1, T>(field.FieldName, field.CanRead, field.CanWrite, field.IsEnumerable,
						field.ElementType, new[] { field.FieldName }, _rootModel ?? _fromModel,
						schemaFieldReference, _entitySchema));
				}
				else
				{
					_fields.Add(new SchemaField<T1, T>(field.FieldName, field.CanRead, field.CanWrite, field.IsEnumerable,
						field.ElementType, _rootPath.Concat(new[] { field.FieldName }).ToArray(), _rootModel ?? _fromModel,
						schemaFieldReference, _entitySchema));
				}
			}

			private ISchemaFieldReference FindSchemaFieldReference<T1>(IField<T1> field)
			{
				return _entitySchema.SchemaFields.FirstOrDefault(
					q => q.ModelPath.SequenceEqual(_rootPath.Concat(new[] { field.FieldName }))
					)?.SchemaFieldReference;
			}

			public SchemaModel BuildSchemaModel()
			{
				string[] selfPath;
				if (_rootPath == null || _rootPath.Length == 0)
					selfPath = new[] { "." };
				else
					selfPath = _rootPath.Concat(new[] { "." }).ToArray();
				return new SchemaModel(_fromModel, _fields.ToArray(), selfPath, _rootModel);
			}
		}
	}

	public class SchemaField<TValue, TEntity> : FieldBase<TValue>, ISourceField, IField<TValue>
		where TEntity : class
	{
		public IModel RootModel { get; }

		public string[] FieldPath { get; }

		private ISourceField[] _fields;
		public ISourceField[] Fields
		{
			get
			{
				if (_fields == null)
				{
					var transformer = new SchemaModel.Transformer<TEntity>(
						FieldPath, RootModel, _entitySchema
						);
					FieldTypeModel.Transform(transformer);
					var fieldTypeSourceModel = transformer.BuildSchemaModel();
					_fields = fieldTypeSourceModel.Fields;
				}
				return _fields;
			}
		}

		public ISchemaFieldReference SchemaFieldReference { get; }

		public override IModel FieldModel => _entitySchema.SchemaModel;

		private readonly EntitySchema<TEntity> _entitySchema;

		public SchemaField(string fieldName, bool canRead, bool canWrite,
			bool isEnumerable, Type elementType, string[] fieldPath, IModel rootModel,
			ISchemaFieldReference schemaFieldReference, EntitySchema<TEntity> entitySchema) :
			base(fieldName, canRead, canWrite, isEnumerable, elementType)
		{
			FieldPath = fieldPath;
			RootModel = rootModel;
			SchemaFieldReference = schemaFieldReference;
			_entitySchema = entitySchema;
		}

		public MappingBinding CreateBinding<TTo>(IMappingBindingFactory bindingFactory, ITargetField toField)
		{
			return bindingFactory.CreateBinding<TValue, TTo>(
				SchemaFieldReference,
				toField.RootModel.GetFieldReference(toField));
		}

		public MappingBinding CreateBinding<TTo, TBindingOption>(IMappingBindingFactory<TBindingOption> bindingFactory, ITargetField toField, TBindingOption bindingOption)
		{
			return bindingFactory.CreateBinding<TValue, TTo>(
				SchemaFieldReference,
				toField.RootModel.GetFieldReference(toField),
				bindingOption);
		}
	}
}
