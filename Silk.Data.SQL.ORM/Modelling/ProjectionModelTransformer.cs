using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
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
			CopyExplicitCast.Instance,
			MapReferenceTypes.Instance,
			CreateInstancesOfPropertiesAsNeeded.Instance,
			CopySameTypes.Instance,
			CastNumericTypes.Instance,
			ConvertToStringWithToString.Instance,
			CopyTryParse.Instance
		};

		private EntityModel _entityModel;
		private Mapping _mapping;
		private readonly Stack<string> _path = new Stack<string>();
		private readonly Stack<IEntityField> _fieldPath = new Stack<IEntityField>();
		private List<IEntityField> _entityFields
			= new List<IEntityField>();
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
			var matchPath = _path.Reverse();
			var binding = _mapping.Bindings.OfType<MappingBinding>().FirstOrDefault(q => q.FromPath.SequenceEqual(matchPath));

			if (binding != null)
			{
				_entityFields.Add(valueField);
			}
		}

		private void ModelSingleRelationshipField<T>(ISingleRelatedObjectField singleRelatedObjectField)
		{
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
			var projectedModel = new ProjectionModel(entityFields.ToArray(), singleRelatedObjectField.RelatedObjectModel.EntityTable, null);

			_entityFields.Add(
				new SingleRelatedObjectField<T>(
					singleRelatedObjectField.FieldName, singleRelatedObjectField.CanRead, singleRelatedObjectField.CanWrite,
					singleRelatedObjectField.IsEnumerable, singleRelatedObjectField.ElementType, singleRelatedObjectField.RelatedObjectModel,
					singleRelatedObjectField.RelatedPrimaryKey, singleRelatedObjectField.LocalColumn, projectedModel
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

			_entityFields.Add(
				new EmbeddedObjectField<T>(
					embeddedObjectField.FieldName, embeddedObjectField.CanRead, embeddedObjectField.CanWrite,
					embeddedObjectField.IsEnumerable, embeddedObjectField.ElementType, fields,
					embeddedObjectField.NullCheckColumn
				));
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
				throw new System.InvalidOperationException("Projections can only be built from EntityModel instances.");
			var mappingBuilder = new MappingBuilder(model, TypeModel.GetModelOf(_projectionType));
			foreach (var convention in _projectionConventions)
				mappingBuilder.AddConvention(convention);
			_mapping = mappingBuilder.BuildMapping();
		}

		public ProjectionModel GetProjectionModel()
		{
			return new ProjectionModel(_entityFields.ToArray(), _entityModel.EntityTable, _mapping);
		}
	}
}
