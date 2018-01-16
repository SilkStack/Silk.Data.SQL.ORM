using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using Silk.Data.SQL.ORM.Modelling.Bindings;
using Silk.Data.SQL.ORM.Modelling.ResourceLoaders;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public static class ViewBuilderExtensions
	{
		public static void DefineManyToManyViewField(this ViewBuilder viewBuilder, ViewBuilder.FieldInfo fieldInfo,
			string[] modelBindingPath = null, string viewFieldName = null,
			params object[] metadata)
		{
			if (viewFieldName == null)
				viewFieldName = fieldInfo.Field.Name;
			viewBuilder.DefineManyToManyViewField(viewFieldName, fieldInfo.Field.DataType,
				modelBindingPath, metadata);
		}

		public static void DefineManyToManyViewField(this ViewBuilder viewBuilder, string viewFieldName, Type fieldDataType,
			string[] modelBindingPath = null,
			params object[] metadata)
		{
			if (modelBindingPath == null)
				modelBindingPath = new[] { viewFieldName };

			viewBuilder.DefineField(viewFieldName,
				new ManyRelatedObjectsBinding(modelBindingPath, new[] { viewFieldName }, fieldDataType),
				fieldDataType, metadata);
		}

		public static void DefineManyToOneViewField(this DataViewBuilder viewBuilder,
			ViewBuilder.FieldInfo fieldInfo,
			 string[] modelBindingPath, string viewFieldName,
			 params object[] metadata)
		{
			var relationshipLoader = GetRelationshipResourceLoader(viewBuilder.ViewDefinition);
			var binding = new SingleRelatedObjectBinding(
				fieldInfo.BindingDirection,
				modelBindingPath,
				new[] { fieldInfo.Field.Name },
				new[] { relationshipLoader },
				viewFieldName
				);

			relationshipLoader.AddField(fieldInfo.Field, modelBindingPath[0], viewFieldName);

			if (!viewBuilder.DomainDefinition.IsReadOnly)
			{
				var tableDefinition = viewBuilder.GetSchemaDefinition().GetEntityTableDefinition();
				tableDefinition.Indexes.Add(new TableIndexDefinition($"{modelBindingPath[0]}Id"));
			}
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
