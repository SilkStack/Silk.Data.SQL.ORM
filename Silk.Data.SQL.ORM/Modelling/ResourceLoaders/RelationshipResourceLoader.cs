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
			var model = modelField.DataTypeModel as TypedModel;
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

			public async Task Run(EntityModel view, ICollection<IContainerReadWriter> sources, MappingContext mappingContext)
			{
				foreach (var viewFieldName in _viewFields)
				{
					var viewField = view.Fields.FirstOrDefault(q => q.Name == viewFieldName.ViewFieldName);
					if (viewField == null)
						continue;

					//  todo: move these items to FieldName to avoid repeated, costly, lookups for all queries
					var fullEntityModel = view.Domain.DataModels.FirstOrDefault(q => q.Schema.EntityTable == view.Schema.EntityTable);
					var fullEntityField = fullEntityModel.Fields.FirstOrDefault(q => q.ModelBinding.ViewFieldPath.SequenceEqual(viewField.ModelBinding.ViewFieldPath));
					var foreignKeyField = fullEntityModel.Fields.FirstOrDefault(q => q.Storage != null && q.Relationship == fullEntityField.Relationship);
					var sourcePath = new[] { viewFieldName.ViewFieldName };

					foreach (var source in sources)
					{
						var instance = ReadObject(source, viewField, foreignKeyField, sourcePath);
						mappingContext.Resources.Store(source, viewFieldName.ViewFieldName, instance);
					}
				}
			}

			private object ReadObject(IContainerReadWriter source, DataField viewField, DataField foreignKeyField,
				string[] sourcePath)
			{
				var relatedObjectField = source.ReadFromPath<object>(
					sourcePath.Take(sourcePath.Length - 1).Concat(new[] { foreignKeyField.Name }).ToArray()
					);
				if (relatedObjectField == null)
					return null;

				var entityModelOfField = viewField.Relationship.ProjectedModel ?? viewField.Relationship.ForeignModel;
				var instance = Activator.CreateInstance(entityModelOfField.EntityType);
				var modelWriter = new ObjectModelReadWriter(TypeModeller.GetModelOf(entityModelOfField.EntityType), instance);
				foreach (var field in entityModelOfField.Fields)
				{
					if (field.Relationship == null)
					{
						modelWriter.WriteToPath<object>(
							new[] { field.Name },
							source.ReadFromPath<object>(sourcePath.Concat(new[] { field.Name }).ToArray())
							);
					}
					else if (field.Relationship.RelationshipType == RelationshipType.ManyToOne)
					{
						if (field.Storage != null) //  is the foreign relationship key
							continue;

						var view = viewField.Relationship.ProjectedModel ?? viewField.Relationship.ForeignModel;
						var fullEntityModel = view.Domain.DataModels.FirstOrDefault(q => q.Schema.EntityTable == view.Schema.EntityTable);
						var fullEntityField = fullEntityModel.Fields.FirstOrDefault(q => q.ModelBinding.ViewFieldPath.SequenceEqual(field.ModelBinding.ViewFieldPath));
						modelWriter.WriteToPath<object>(
							new[] { field.Name },
							ReadObject(
								source, field,
								fullEntityModel.Fields.FirstOrDefault(q => q.Storage != null && q.Relationship == fullEntityField.Relationship),
								sourcePath.Concat(new[] { field.Name }).ToArray())
							);
					}
				}
				return instance;
			}
		}
	}
}
