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

		public void AddField(ModelField modelField, string modelFieldName, string viewFieldName)
		{
			var model = modelField.ParentModel as TypedModel;
			if (model == null)
				throw new InvalidOperationException("Only type models are supported.");
			var relatedObjectMapper = GetRelatedObjectMapper(model.DataType);
			relatedObjectMapper.AddField(
				new FieldName(viewFieldName, modelFieldName));
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
			foreach (var relatedObjectMapper in _relatedObjectMappers)
			{
				await relatedObjectMapper.Run(view, sources, mappingContext)
					.ConfigureAwait(false);
			}
		}

		private class FieldName
		{
			public string ViewFieldName { get; }
			public string ModelFieldName { get; }

			public FieldName(string viewFieldName, string modelFieldName)
			{
				ViewFieldName = viewFieldName;
				ModelFieldName = modelFieldName;
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

			public async Task Run(IView view, ICollection<IContainerReadWriter> sources, MappingContext mappingContext)
			{
				foreach (var viewFieldName in _viewFields)
				{
					var fieldPath = new[] { viewFieldName.ViewFieldName };
					foreach (var source in sources)
					{
						var relatedObjectField = source.ReadFromPath<object>(fieldPath);
						if (relatedObjectField == null)
							continue;

						var instance = Activator.CreateInstance(ModelType);
						var modelWriter = new ObjectModelReadWriter(Model, instance);
						foreach (var field in Model.Fields)
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
