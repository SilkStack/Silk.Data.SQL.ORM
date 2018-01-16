using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Silk.Data.Modelling;
using Silk.Data.Modelling.ResourceLoaders;

namespace Silk.Data.SQL.ORM.Modelling.ResourceLoaders
{
	public class RelationshipResourceLoader : IResourceLoader
	{
		private readonly List<RelatedObjectMapper> _relatedObjectMappers
			= new List<RelatedObjectMapper>();

		public void AddField(ModelField modelField, string modelFieldName, string viewFieldName,
			string nullCheckFieldName)
		{
			var model = modelField.DataTypeModel as TypedModel;
			if (model == null)
				throw new InvalidOperationException("Only type models are supported.");
			var relatedObjectMapper = GetRelatedObjectMapper(model.DataType);
			relatedObjectMapper.AddField(
				new FieldName(viewFieldName, modelFieldName, nullCheckFieldName));
		}

		private RelatedObjectMapper GetRelatedObjectMapper(Type modelType)
		{
			var ret = _relatedObjectMappers.FirstOrDefault(q => q.ModelType == modelType);
			if (ret != null)
				return ret;
			ret = new RelatedObjectMapper(modelType);
			_relatedObjectMappers.Add(ret);
			return ret;
		}

		public async Task LoadResourcesAsync(IView view, ICollection<IContainerReadWriter> sources, MappingContext mappingContext)
		{
			if (view is EntityModel entityModel)
			{
				foreach (var relatedObjectMapper in _relatedObjectMappers)
				{
					await relatedObjectMapper.Run(entityModel, sources, mappingContext)
						.ConfigureAwait(false);
				}
			}
		}

		private class FieldName
		{
			public string ViewFieldName { get; }
			public string ModelFieldName { get; }
			public string NullCheckFieldName { get; }

			public FieldName(string viewFieldName, string modelFieldName,
				string nullCheckFieldName)
			{
				ViewFieldName = viewFieldName;
				ModelFieldName = modelFieldName;
				NullCheckFieldName = nullCheckFieldName;
			}
		}

		private class RelatedObjectMapper
		{
			public Type ModelType { get; }
			public TypedModel Model { get; }
			private readonly List<FieldName> _viewFields = new List<FieldName>();

			public RelatedObjectMapper(Type modelType)
			{
				ModelType = modelType;
				Model = TypeModeller.GetModelOf(modelType);
			}

			public void AddField(FieldName viewFieldName)
			{
				_viewFields.Add(viewFieldName);
			}

			public async Task Run(EntityModel view, ICollection<IContainerReadWriter> sources, MappingContext mappingContext)
			{
				foreach (var viewFieldName in _viewFields)
				{
					var viewField = view.Fields.FirstOrDefault(q => q.Name == viewFieldName.ViewFieldName);
					if (viewField == null)
						continue;

					var fullEntityModel = view.Domain.DataModels.FirstOrDefault(q => q.Schema.EntityTable == view.Schema.EntityTable);
					var fullEntityField = fullEntityModel.Fields.FirstOrDefault(q => q.ModelBinding.ViewFieldPath.SequenceEqual(viewField.ModelBinding.ViewFieldPath));
					var foreignKeyField = fullEntityModel.Fields.FirstOrDefault(q => q.Storage != null && q.Relationship == fullEntityField.Relationship);

					foreach (var source in sources)
					{
						var relatedObjectField = foreignKeyField.ModelBinding.ReadValue<object>(source);
						if (relatedObjectField == null)
							continue;

						var instance = Activator.CreateInstance(ModelType);
						var modelWriter = new ObjectModelReadWriter(Model, instance);
						var entityModelOfField = viewField.Relationship.ProjectedModel ?? viewField.Relationship.ForeignModel;
						foreach (var field in entityModelOfField.Fields)
						{
							modelWriter.WriteToPath<object>(
								new[] { field.Name },
								source.ReadFromPath<object>(new[] { viewFieldName.ViewFieldName, field.Name })
								);
						}
						mappingContext.Resources.Store(source, viewFieldName.ViewFieldName, instance);
					}
				}
			}
		}
	}
}
