﻿using Silk.Data.Modelling;
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
			var fromProjectionModel = fromModel.FromModel as ProjectionModel;
			if (fromProjectionModel == null)
				return;

			foreach (var toField in toModel.Fields.Where(q => q.CanWrite && !q.IsEnumerable && MapReferenceTypes.IsReferenceType(q.FieldType)))
			{
				HandleField(toField);
			}

			void HandleField(ITargetField toField)
			{
				var path = toField.FieldPath;
				var entityField = GetField(fromProjectionModel, path);

				if (entityField is IEmbeddedObjectField embeddedObjectField)
				{
					foreach (var subField in embeddedObjectField.EmbeddedFields)
					{
						var subPath = path.Concat(new[] { subField.FieldName }).ToArray();
						var subToField = toModel.GetField(subPath);

						if (subToField != null)
						{
							HandleField(subToField);
						}
					}
				}
				else if (entityField is ISingleRelatedObjectField singleRelatedObjectField)
				{
					if (!builder.IsBound(toField))
					{
						var ctor = toField.FieldType.GetTypeInfo().DeclaredConstructors
							.FirstOrDefault(q => q.GetParameters().Length == 0);
						if (ctor == null)
							return;

						builder
							.Bind(toField)
							.AssignUsing<CreateSingleRelatedInstanceIfPresent, ConstructorInfo>(ctor);
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

	public class CreateSingleRelatedInstanceIfPresent : IAssignmentBindingFactory<ConstructorInfo>
	{
		public AssignmentBinding CreateBinding<TTo>(ITargetField toField, ConstructorInfo bindingOption)
		{
			return new CreateSingleRelatedInstanceIfPresent<TTo>(bindingOption, toField.FieldPath);
		}
	}

	public class CreateSingleRelatedInstanceIfPresent<T> : AssignmentBinding
	{
		private readonly CreateInstanceIfNull<T> _impl;

		public CreateSingleRelatedInstanceIfPresent(ConstructorInfo constructorInfo, string[] toPath) : base(toPath)
		{
			_impl = new CreateInstanceIfNull<T>(constructorInfo, toPath);
		}

		public override void AssignBindingValue(IModelReadWriter from, IModelReadWriter to)
		{
			var nullCheckObj = from.ReadField<object>(ToPath, 0);
			if (nullCheckObj != null)
				_impl.PerformBinding(from, to);
		}
	}
}