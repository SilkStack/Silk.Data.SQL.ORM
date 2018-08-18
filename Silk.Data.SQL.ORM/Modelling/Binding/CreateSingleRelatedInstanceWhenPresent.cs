using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
using System.Linq;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Modelling.Binding
{
	public class CreateSingleRelatedInstanceWhenPresent : IMappingConvention
	{
		public static CreateSingleRelatedInstanceWhenPresent Instance { get; }
			= new CreateSingleRelatedInstanceWhenPresent();

		public void CreateBindings(SourceModel fromModel, TargetModel toModel, MappingBuilder builder)
		{
			var fromProjectionModel = fromModel.FromModel as IProjectionModel;
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
						var ctor = fromField.FieldType.GetTypeInfo().DeclaredConstructors
							.FirstOrDefault(q => q.GetParameters().Length == 0);
						if (ctor == null)
							return;

						builder
							.Bind(toField)
							.AssignUsing<CreateSingleRelatedInstanceIfPresent, (ConstructorInfo,string[])>((ctor,path));
					}
				}
			}
		}

		private IEntityField GetField(IProjectionModel model, string[] path)
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

	public class CreateSingleRelatedInstanceIfPresent : IAssignmentBindingFactory<(ConstructorInfo ctor, string[] readPath)>
	{
		public AssignmentBinding CreateBinding<TTo>(ITargetField toField, (ConstructorInfo ctor, string[] readPath) bindingOption)
		{
			return new CreateSingleRelatedInstanceIfPresent<TTo>(bindingOption.ctor, bindingOption.readPath, toField.FieldPath);
		}
	}

	public class CreateSingleRelatedInstanceIfPresent<T> : AssignmentBinding
	{
		private readonly string[] _readPath;
		private readonly CreateInstanceIfNull<T> _impl;

		public CreateSingleRelatedInstanceIfPresent(ConstructorInfo constructorInfo, string[] readPath, string[] toPath) : base(toPath)
		{
			_readPath = readPath;
			_impl = new CreateInstanceIfNull<T>(constructorInfo, toPath);
		}

		public override void AssignBindingValue(IModelReadWriter from, IModelReadWriter to)
		{
			var nullCheckObj = from.ReadField<object>(_readPath, 0);
			if (nullCheckObj != null)
				_impl.PerformBinding(from, to);
		}
	}
}
