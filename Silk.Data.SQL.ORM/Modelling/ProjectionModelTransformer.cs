using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
using Silk.Data.SQL.ORM.Modelling.Binding;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class ProjectionModelTransformer : IModelTransformer
	{
		private readonly static IMappingConvention[] _projectionConventions = new IMappingConvention[]
		{
			CreateInstanceAsNeeded.Instance,
			CreateEmbeddedInstanceUsingNotNullColumn.Instance,
			CreateSingleRelatedInstanceWhenPresent.Instance,
			CopyValueFields.Instance
		};

		private EntityModel _entityModel;
		private Mapping _mapping;
		private readonly Stack<string> _path = new Stack<string>();
		private readonly Stack<IEntityField> _fieldPath = new Stack<IEntityField>();
		private List<IEntityField> _entityFields
			= new List<IEntityField>();
		private TargetModel _toModel;
		private readonly Type _projectionType;

		public ProjectionModelTransformer(Type projectionType)
		{
			_projectionType = projectionType;
		}

		private MappingBinding FindBinding(IEnumerable<string> matchPath)
		{
			return _mapping.Bindings.OfType<MappingBinding>()
				.FirstOrDefault(q => q.FromPath.SequenceEqual(matchPath));
		}

		private void ModelPrimitiveField<T>(IValueField valueField)
		{
			var binding = _mapping.Bindings.OfType<MappingBinding>()
				.FirstOrDefault(q => q.FromPath.SequenceEqual(_path.Reverse()));

			if (binding != null)
			{
				_entityFields.Add(new ProjectedValueField<T>(
					valueField.FieldName, valueField.CanRead, valueField.CanWrite, valueField.IsEnumerable,
					valueField.ElementType, valueField.Column, binding.ToPath
					));
			}
		}

		private void ModelSingleRelationshipField<T>(ISingleRelatedObjectField singleRelatedObjectField)
		{
			var toField = _toModel.GetField(_path.Reverse().ToArray());
			var existingFields = _entityFields;
			_entityFields = new List<IEntityField>();

			foreach (var field in singleRelatedObjectField.RelatedObjectModel.Fields)
			{
				field.Transform(this);
			}

			var entityFields = _entityFields;
			_entityFields = existingFields;

			if (entityFields.Count < 1)
				return;
			var projectedModel = new ViewModel(entityFields.ToArray(), singleRelatedObjectField.RelatedObjectModel.EntityTable, toField?.FieldType);

			IValueField primaryKeyField = singleRelatedObjectField.RelatedPrimaryKey;
			if (primaryKeyField != null)
			{
				existingFields = _entityFields;
				_entityFields = new List<IEntityField>();
				primaryKeyField.Transform(this);
				primaryKeyField = _entityFields.OfType<IValueField>().FirstOrDefault();
				_entityFields = existingFields;
			}

			_entityFields.Add(
				new SingleRelatedObjectField<T>(
					singleRelatedObjectField.FieldName, singleRelatedObjectField.CanRead, singleRelatedObjectField.CanWrite,
					singleRelatedObjectField.IsEnumerable, singleRelatedObjectField.ElementType, singleRelatedObjectField.RelatedObjectModel,
					primaryKeyField, singleRelatedObjectField.LocalColumn, projectedModel
				));
		}

		private void ModelEmbeddedObjectField<T>(IEmbeddedObjectField embeddedObjectField)
		{
			var existingFields = _entityFields;
			_entityFields = new List<IEntityField>();

			foreach (var field in embeddedObjectField.EmbeddedFields)
			{
				field.Transform(this);
			}

			var fields = _entityFields;
			_entityFields = existingFields;

			if (fields.Count < 1)
				return;

			var sourceField = _toModel.GetField(_path.Reverse().ToArray());
			var fieldType = sourceField?.FieldType ?? typeof(T);

			_entityFields.Add(
				Activator.CreateInstance(
					typeof(EmbeddedObjectField<>).MakeGenericType(fieldType),
					embeddedObjectField.FieldName, embeddedObjectField.CanRead, embeddedObjectField.CanWrite,
					embeddedObjectField.IsEnumerable, embeddedObjectField.ElementType, fields,
					embeddedObjectField.NullCheckColumn
				) as IEntityField);
		}

		public void VisitField<T>(IField<T> field)
		{
			if (field is IEntityField entityField)
			{
				_fieldPath.Push(entityField);
			}
			else
			{
				throw new InvalidOperationException("Non entity field encountered.");
			}
			_path.Push(field.FieldName);

			if (field is IValueField valueField)
			{
				ModelPrimitiveField<T>(valueField);
			}
			else if (field is ISingleRelatedObjectField singleRelatedObjectField)
			{
				ModelSingleRelationshipField<T>(singleRelatedObjectField);
			}
			else if (field is IEmbeddedObjectField embeddedObjectField)
			{
				ModelEmbeddedObjectField<T>(embeddedObjectField);
			}

			_fieldPath.Pop();
			_path.Pop();
		}

		public void VisitModel<TField>(IModel<TField> model) where TField : IField
		{
			_entityModel = model as EntityModel;
			if (_entityModel == null)
				throw new InvalidOperationException("Projections can only be built from EntityModel instances.");
			_toModel = TypeModel.GetModelOf(_projectionType).TransformToTargetModel();
			var mappingBuilder = new MappingBuilder(model, TypeModel.GetModelOf(_projectionType));
			foreach (var convention in _projectionConventions)
				mappingBuilder.AddConvention(convention);
			_mapping = mappingBuilder.BuildMapping();
		}

		public ProjectionModel GetProjectionModel()
		{
			return new ProjectionModel(_entityFields.ToArray(), _entityModel.EntityTable, _mapping);
		}

		private class ProjectedValueField<T> : FieldBase<T>, IProjectedValueField
		{
			public string[] Path { get; }
			public Column Column { get; }

			public ProjectedValueField(string fieldName, bool canRead, bool canWrite,
				bool isEnumerable, Type elementType, Column column, string[] path) :
				base(fieldName, canRead, canWrite, isEnumerable, elementType)
			{
				Path = path;
				Column = column;
			}
		}
	}
}
