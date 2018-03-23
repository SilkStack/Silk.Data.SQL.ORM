using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
using System.Linq;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Modelling.Binding
{
	public class CreateEmbeddedInstanceUsingNotNullColumn : IMappingConvention
	{
		public static CreateEmbeddedInstanceUsingNotNullColumn Instance { get; }
			= new CreateEmbeddedInstanceUsingNotNullColumn();

		public void CreateBindings(SourceModel fromModel, TargetModel toModel, MappingBuilder builder)
		{
			var fromProjectionModel = fromModel.FromModel as ProjectionModel;
			if (fromProjectionModel == null)
				return;

			foreach (var fromField in fromModel.Fields.Where(q => q.CanRead && !q.IsEnumerable && MapReferenceTypes.IsReferenceType(q.FieldType)))
			{
				HandleField(fromField);
			}

			void HandleField(ISourceField fromField)
			{
				var path = fromField.FieldPath;
				var entityField = GetField(fromProjectionModel, path);

				if (entityField is IEmbeddedObjectField embeddedObjectField)
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
						var ctor = toField.FieldType.GetTypeInfo().DeclaredConstructors
							.FirstOrDefault(q => q.GetParameters().Length == 0);
						builder
							.Bind(toField)
							.AssignUsing<CreateEmbeddedInstanceIfPresent, ConstructorInfo>(ctor);
					}

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

	public class CreateEmbeddedInstanceIfPresent : IAssignmentBindingFactory<ConstructorInfo>
	{
		public AssignmentBinding CreateBinding<TTo>(ITargetField toField, ConstructorInfo bindingOption)
		{
			return new CreateEmbeddedInstanceIfPresent<TTo>(bindingOption, toField.FieldPath);
		}
	}

	public class CreateEmbeddedInstanceIfPresent<T> : AssignmentBinding
	{
		private readonly CreateInstanceIfNull<T> _impl;

		public CreateEmbeddedInstanceIfPresent(ConstructorInfo constructorInfo, string[] toPath) : base(toPath)
		{
			_impl = new CreateInstanceIfNull<T>(constructorInfo, toPath);
		}

		public override void AssignBindingValue(IModelReadWriter from, IModelReadWriter to)
		{
			var nullCheckObj = from.ReadField<bool>(ToPath, 0);
			if (nullCheckObj)
				_impl.PerformBinding(from, to);
		}
	}
}
