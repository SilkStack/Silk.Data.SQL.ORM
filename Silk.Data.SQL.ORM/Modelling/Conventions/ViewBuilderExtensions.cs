using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Modelling.Bindings;
using Silk.Data.SQL.ORM.Modelling.ResourceLoaders;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public static class ViewBuilderExtensions
	{
		public static void DefineManyToOneViewField(this ViewBuilder viewBuilder,
			ViewBuilder.FieldInfo fieldInfo,
			 string[] modelBindingPath, string viewFieldName,
			 params object[] metadata)
		{
			var relationshipLoader = GetRelationshipResourceLoader(viewBuilder.ViewDefinition);
			var binding = new RelatedObjectBinding(
				fieldInfo.BindingDirection,
				modelBindingPath,
				new[] { fieldInfo.Field.Name },
				new[] { relationshipLoader },
				viewFieldName
				);

			//  todo: this null check field only works because of the workings of the DataDomain as it's built
			//      this should be refactored away at some point
			relationshipLoader.AddField(fieldInfo.Field, modelBindingPath[0],
				viewFieldName, $"{modelBindingPath[0]}Id");

			viewBuilder.DefineField(viewFieldName, binding, fieldInfo.Field.DataType, metadata);
		}

		private static RelationshipResourceLoader GetRelationshipResourceLoader(ViewDefinition viewDefinition)
		{
			var relationshipLoader = viewDefinition.ResourceLoaders
				.OfType<RelationshipResourceLoader>()
				.FirstOrDefault();
			if (relationshipLoader == null)
			{
				relationshipLoader = new RelationshipResourceLoader();
				viewDefinition.ResourceLoaders.Add(relationshipLoader);
			}
			return relationshipLoader;
		}
	}
}
