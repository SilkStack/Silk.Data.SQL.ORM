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

		public static SchemaModel Create<T>(EntitySchema<T> entitySchema)
			where T : class
			=> new Transformer<T>(null, TypeModel.GetModelOf<T>(), entitySchema).BuildSchemaModel();

		private class Transformer<T> : IModelTransformer
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
				if (schemaFieldReference == null)
					return;

				if (_rootPath == null || _rootPath.Length == 0)
					_fields.Add(new SchemaField<T1>(field.FieldName, field.CanRead, field.CanWrite, field.IsEnumerable,
						field.ElementType, new[] { field.FieldName }, _rootModel ?? _fromModel, schemaFieldReference));
				else
					_fields.Add(new SchemaField<T1>(field.FieldName, field.CanRead, field.CanWrite, field.IsEnumerable,
						field.ElementType, _rootPath.Concat(new[] { field.FieldName }).ToArray(), _rootModel ?? _fromModel,
						schemaFieldReference));
			}

			private ISchemaFieldReference FindSchemaFieldReference<T1>(IField<T1> field)
			{
				return _entitySchema.SchemaFields.FirstOrDefault(
					q => q.ModelPath.SequenceEqual(_rootPath.Concat(new[] { field.FieldName }))
					)?.SchemaFieldReference;
			}

			public SchemaModel BuildSchemaModel()
			{
				_rootModel.Transform(this);
				string[] selfPath;
				if (_rootPath == null || _rootPath.Length == 0)
					selfPath = new[] { "." };
				else
					selfPath = _rootPath.Concat(new[] { "." }).ToArray();
				return new SchemaModel(_fromModel, _fields.ToArray(), selfPath, _rootModel);
			}
		}
	}

	public class SchemaField<T> : FieldBase<T>, ISourceField, IField<T>
	{
		public IModel RootModel { get; }

		public string[] FieldPath { get; }

		public ISourceField[] Fields { get; } = new ISourceField[0];

		public ISchemaFieldReference SchemaFieldReference { get; }

		public SchemaField(string fieldName, bool canRead, bool canWrite,
			bool isEnumerable, Type elementType, string[] fieldPath, IModel rootModel,
			ISchemaFieldReference schemaFieldReference) :
			base(fieldName, canRead, canWrite, isEnumerable, elementType)
		{
			FieldPath = fieldPath;
			RootModel = rootModel;
			SchemaFieldReference = schemaFieldReference;
		}

		public MappingBinding CreateBinding<TTo>(IMappingBindingFactory bindingFactory, ITargetField toField)
		{
			return bindingFactory.CreateBinding<T, TTo>(
				SchemaFieldReference,
				toField.RootModel.GetFieldReference(toField));
		}

		public MappingBinding CreateBinding<TTo, TBindingOption>(IMappingBindingFactory<TBindingOption> bindingFactory, ITargetField toField, TBindingOption bindingOption)
		{
			return bindingFactory.CreateBinding<T, TTo>(
				SchemaFieldReference,
				toField.RootModel.GetFieldReference(toField),
				bindingOption);
		}
	}
}
