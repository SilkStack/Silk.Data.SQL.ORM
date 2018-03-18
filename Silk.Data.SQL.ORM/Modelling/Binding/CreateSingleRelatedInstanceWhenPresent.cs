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
			foreach (var toField in toModel.Fields.Where(q => q.CanWrite && !q.IsEnumerable && MapReferenceTypes.IsReferenceType(q.FieldType) && !builder.IsBound(q)))
			{
				var ctor = toField.FieldType.GetTypeInfo().DeclaredConstructors
					.FirstOrDefault(q => q.GetParameters().Length == 0);
				if (ctor == null)
				{
					continue;
				}

				builder
					.Bind(toField)
					.AssignUsing<CreateSingleRelatedInstanceIfPresent, ConstructorInfo>(ctor);
			}
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
