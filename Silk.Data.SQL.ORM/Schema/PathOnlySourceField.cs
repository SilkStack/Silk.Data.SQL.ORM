using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
using System;

namespace Silk.Data.SQL.ORM.Schema
{
	internal class PathOnlySourceField : ISourceField
	{
		public IModel RootModel => throw new NotImplementedException();

		public string[] FieldPath { get; }

		public ISourceField[] Fields => throw new NotImplementedException();

		public string FieldName => throw new NotImplementedException();

		public Type FieldType => throw new NotImplementedException();

		public bool CanRead => throw new NotImplementedException();

		public bool CanWrite => throw new NotImplementedException();

		public bool IsEnumerable => throw new NotImplementedException();

		public Type ElementType => throw new NotImplementedException();

		public TypeModel FieldTypeModel => throw new NotImplementedException();

		public IModel FieldModel => throw new NotImplementedException();

		public PathOnlySourceField(string[] fieldPath)
		{
			FieldPath = fieldPath;
		}

		public MappingBinding CreateBinding<TTo>(IMappingBindingFactory bindingFactory, ITargetField toField)
		{
			throw new NotImplementedException();
		}

		public MappingBinding CreateBinding<TTo, TBindingOption>(IMappingBindingFactory<TBindingOption> bindingFactory, ITargetField toField, TBindingOption bindingOption)
		{
			throw new NotImplementedException();
		}

		public void Transform(IModelTransformer transformer)
		{
			throw new NotImplementedException();
		}
	}
}
