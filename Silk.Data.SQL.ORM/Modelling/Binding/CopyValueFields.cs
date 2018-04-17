using Silk.Data.Modelling.Mapping;
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

			foreach (var fromField in fromModel.Fields.Where(q => q.CanRead && !q.IsEnumerable))
			{
				HandleField(fromField);
			}

			void HandleField(ISourceField fromField)
			{
				var path = fromField.FieldPath;
				var entityField = GetField(fromProjectionModel, path);

				if (entityField is IEmbeddedObjectField embeddedObjectField)
				{
					foreach (var subField in embeddedObjectField.EmbeddedFields)
					{
						var subPath = path.Concat(new[] { subField.FieldName }).ToArray();
						var subToField = fromModel.GetField(subPath);

						if (subToField != null)
						{
							HandleField(subToField);
						}
					}
				}
				else if (entityField is ISingleRelatedObjectField singleRelatedObjectField)
				{
					foreach (var subField in singleRelatedObjectField.RelatedObjectModel.Fields)
					{
						var subPath = path.Concat(new[] { subField.FieldName }).ToArray();
						var subToField = fromModel.GetField(subPath);

						if (subToField != null)
						{
							HandleField(subToField);
						}
					}
				}
				else if (entityField is IValueField valueField)
				{
					var toField = toModel.GetField(path);
					if (toField == null)
					{
						var flatPath = string.Join("", path);
						var candidatePaths = ConventionUtilities.GetPaths(flatPath);
						foreach (var candidatePath in candidatePaths)
						{
							toField = toModel.GetField(candidatePath);
							if (toField != null)
								break;
						}
					}
					if (toField == null)
						return;
					if (!builder.IsBound(toField))
					{
						builder
							.Bind(toField)
							.From(fromField)
							.MapUsing<ConditionalCopyBinding>();
					}
				}
			}
		}

		private static IEntityField GetField(ProjectionModel model, string[] path)
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
