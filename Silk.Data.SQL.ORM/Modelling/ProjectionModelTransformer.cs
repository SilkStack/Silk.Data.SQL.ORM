using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class ProjectionModelTransformer<TProjection> : IModelTransformer
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
		private readonly List<IEntityField> _entityFields
			= new List<IEntityField>();

		private MappingBinding FindBinding(IEnumerable<string> matchPath)
		{
			return _mapping.Bindings.OfType<MappingBinding>()
				.FirstOrDefault(q => q.FromPath.SequenceEqual(matchPath));
		}

		public void VisitField<T>(IField<T> field)
		{
			if (field is IEntityField entityField)
			{
				_fieldPath.Push(entityField);
			}
			else
			{
				throw new System.InvalidOperationException("Non entity field encountered.");
			}
			_path.Push(field.FieldName);

			var matchPath = _path.Reverse();
			var binding = _mapping.Bindings.OfType<MappingBinding>().FirstOrDefault(q => q.FromPath.SequenceEqual(matchPath));

			if (field is IValueField valueField)
			{
				if (binding != null)
				{
					if (binding.FromPath.SequenceEqual(binding.ToPath))
					{
						_entityFields.Add(
							new ValueField<T>(field.FieldName, field.CanRead, field.CanWrite, field.IsEnumerable, field.ElementType, valueField.Column)
							);
					}
					else
					{
						_entityFields.Add(
							new ProjectionField<T>(string.Join("_", binding.ToPath), field.CanRead, field.CanWrite, field.IsEnumerable, field.ElementType, _fieldPath.Reverse())
							);
					}
				}
			}
			else if (field is IEmbeddedObjectField embeddedObjectField)
			{
				if (binding != null)
				{
					_entityFields.Add(
						new ProjectionField<T>(string.Join("_", binding.ToPath), field.CanRead, field.CanWrite, field.IsEnumerable, field.ElementType, _fieldPath.Reverse())
						);
				}

				foreach (var subField in embeddedObjectField.EmbeddedFields)
					subField.Transform(this);
			}
			else if (field is ISingleRelatedObjectField singleRelatedObjectField)
			{
				if (binding != null)
				{
					_entityFields.Add(
						new ProjectionField<T>(string.Join("_", binding.ToPath), field.CanRead, field.CanWrite, field.IsEnumerable, field.ElementType, _fieldPath.Reverse())
						);
				}

				foreach (var subField in singleRelatedObjectField.RelatedObjectModel.Fields)
					subField.Transform(this);
			}
			else if (field is IManyRelatedObjectField manyRelatedObjectField)
			{

			}

			_fieldPath.Pop();
			_path.Pop();
		}

		public void VisitModel<TField>(IModel<TField> model) where TField : IField
		{
			_entityModel = model as EntityModel;
			if (_entityModel == null)
				throw new System.InvalidOperationException("Projections can only be built from EntityModel instances.");
			var mappingBuilder = new MappingBuilder(model, TypeModel.GetModelOf<TProjection>());
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
