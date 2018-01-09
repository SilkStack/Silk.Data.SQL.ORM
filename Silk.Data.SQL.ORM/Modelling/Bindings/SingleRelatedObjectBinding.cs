using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using Silk.Data.Modelling.ResourceLoaders;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.Bindings
{
	public class SingleRelatedObjectBinding : ModelBinding
	{
		private readonly string _fieldName;

		public override BindingDirection Direction { get; }

		public SingleRelatedObjectBinding(BindingDirection bindingDirection, string[] modelFieldPath,
			string[] viewFieldPath, IResourceLoader[] resourceLoaders,
			string fieldName)
			: base(modelFieldPath, viewFieldPath, resourceLoaders)
		{
			_fieldName = fieldName;
			Direction = bindingDirection;
		}

		public override T ReadTransformedValue<T>(IContainerReadWriter from, MappingContext mappingContext)
		{
			if (mappingContext.BindingDirection == BindingDirection.ModelToView)
				return ReadValue<T>(from);
			return (T)mappingContext.Resources.Retrieve(from, _fieldName);
		}

		public override void CopyBindingValue(IContainerReadWriter from, IContainerReadWriter to, MappingContext mappingContext)
		{
			if (mappingContext.BindingDirection == BindingDirection.ModelToView)
			{
				base.CopyBindingValue(from, to, mappingContext);
			}
			else
			{
				object value = ReadTransformedValue<object>(from, mappingContext);
				to.WriteToPath<object>(ModelFieldPath.Take(1).ToArray(), value);
			}
		}
	}
}
