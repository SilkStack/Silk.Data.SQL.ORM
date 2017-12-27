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

			relationshipLoader.AddField(fieldInfo.Field, modelBindingPath[0], viewFieldName);

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
