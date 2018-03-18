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
				var entityField = GetField(fromProjectionModel, fromField.FieldPath);
				if (entityField is IValueField)
				{
					builder
						.Bind(toField)
						.From(fromField)
						.MapUsing<CopyBinding>();
				}
				else if (entityField is ISingleRelatedObjectField singleRelatedObjectField)
				{
					CrawlSingleRelatedObjectField(singleRelatedObjectField, fromField.FieldPath);
				}
			}

			void CrawlSingleRelatedObjectField(ISingleRelatedObjectField singleRelatedObjectField, string[] path)
			{
				foreach (var subField in singleRelatedObjectField.RelatedObjectModel.Fields)
				{
					var subPath = path.Concat(new[] { subField.FieldName }).ToArray();

					var subToField = toModel.GetField(subPath);
					var subFromField = fromModel.GetField(subPath);
					var entityField = GetField(fromProjectionModel, subPath);

					if (entityField is IValueField)
					{
						builder
							.Bind(subToField)
							.From(subFromField)
							.MapUsing<CopyBinding>();
					}
					else if (entityField is ISingleRelatedObjectField subSingleRelatedObjectField)
					{
						CrawlSingleRelatedObjectField(subSingleRelatedObjectField, path);
					}
				}
			}
		}

		private IEntityField GetField(ProjectionModel model, string[] path)
		{
			IEntityField ret = null;
			foreach (var segment in path)
			{
				ret = model.Fields.FirstOrDefault(q => q.FieldName == segment);
				if (ret == null)
					break;
				if (ret is ISingleRelatedObjectField singleRelatedObjectField)
					model = singleRelatedObjectField.RelatedObjectModel;
			}
			return ret;
		}
	}
}
