using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.Binding
{
	public class CopyValueFields : IMappingConvention
	{
		public static CopyValueFields Instance { get; } = new CopyValueFields();

		public void CreateBindings(SourceModel fromModel, TargetModel toModel, MappingBuilder builder)
		{
			var fromProjectionModel = fromModel.FromModel as ProjectionModel;
			if (fromProjectionModel == null)
				return;

			foreach (var (fromField, toField) in ConventionUtilities.GetBindCandidatePairs(fromModel, toModel, builder))
			{
				HandleFieldPair(fromField, toField);
			}

			void HandleFieldPair(ISourceField fromField, ITargetField toField)
			{
				var path = fromField.FieldPath;
				var entityField = GetField(fromProjectionModel, path);
				if (entityField is IValueField)
				{
					if (!builder.IsBound(toField))
					{
						builder
							.Bind(toField)
							.From(fromField)
							.MapUsing<CopyBinding>();
					}
				}
				else if (entityField is IEmbeddedObjectField embeddedObjectField)
				{
					foreach (var subField in embeddedObjectField.EmbeddedFields)
					{
						var subPath = path.Concat(new[] { subField.FieldName }).ToArray();
						var subToField = toModel.GetField(subPath);
						var subFromField = fromModel.GetField(subPath);

						if (subToField != null && subFromField != null)
						{
							HandleFieldPair(subFromField, subToField);
						}
					}
				}
				else if (entityField is ISingleRelatedObjectField singleRelatedObjectField)
				{
					foreach (var subField in singleRelatedObjectField.RelatedObjectModel.Fields)
					{
						var subPath = path.Concat(new[] { subField.FieldName }).ToArray();
						var subToField = toModel.GetField(subPath);
						var subFromField = fromModel.GetField(subPath);

						if (subToField != null && subFromField != null)
						{
							HandleFieldPair(subFromField, subToField);
						}
					}
				}
			}
		}

		private IEntityField GetField(ProjectionModel model, string[] path)
		{
			IEntityField ret = null;
			var fields = model.Fields;
			foreach (var segment in path)
			{
				ret = fields.FirstOrDefault(q => q.FieldName == segment);
				if (ret == null)
					break;
				if (ret is ISingleRelatedObjectField singleRelatedObjectField)
					fields = singleRelatedObjectField.RelatedObjectModel.Fields;
				else if (ret is IEmbeddedObjectField embeddedObjectField)
					fields = embeddedObjectField.EmbeddedFields;
			}
			return ret;
		}
	}
}
